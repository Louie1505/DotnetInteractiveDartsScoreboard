from ultralytics import YOLO
from get_scores import GetScores
import cv2
import numpy as np
import time
import platform

class VideoProcessing:
    def __init__(self, model_dir="weights.pt"):
        self.model = YOLO(model_dir)
        self.predict = GetScores(model_dir)
    
    def _debug_detections(self, result, calibration_coords):
        """Debug function to print what YOLO is detecting"""
        classes = result.boxes.cls
        boxes = result.boxes.xywhn
        conf = result.boxes.conf
        
        print(f"=== Frame Debug ===")
        print(f"Total detections: {len(classes)}")
        
        class_names = {0: '20', 1: '3', 2: '11', 3: '6', 4: 'dart'}
        
        for i in range(len(classes)):
            class_id = int(classes[i].item())
            confidence = conf[i].item()
            box = boxes[i]
            class_name = class_names.get(class_id, 'unknown')
            
            print(f"  Detection {i}: Class={class_name} ({class_id}), Conf={confidence:.3f}, Box=[{box[0]:.3f}, {box[1]:.3f}]")
        
        # Show calibration status
        valid_calibration_points = np.sum(np.all(calibration_coords != -1, axis=1))
        print(f"Valid calibration points: {valid_calibration_points}/6")
        
        for i, coord in enumerate(calibration_coords):
            point_names = ['20', '3', '11', '6', '9', '15']  # based on the indices
            status = "FOUND" if not np.all(coord == -1) else "MISSING"
            print(f"  {point_names[i]}: {status} - {coord}")
        
        print("==================")

    def _distance(self, coord1, coord2):
        return np.sqrt(np.sum((coord1 - coord2) ** 2))

    def _assess_visit(self, darts):
        darts = [dart for dart in darts if dart != '']
        score=0
        for dart in darts:
            score += self.scorer.get_score_for_dart(dart)
        
        remaining = self.scorer.scores[self.scorer.current_player] - score

        if remaining <= 1 or len(darts) == 3:
            if self.wait_for_dart_removal == False:
                self.scorer.read_score(score)

            self.wait_for_dart_removal = True

        if (remaining == 0 and darts[-1][0] != 'D') or remaining == 1 or remaining < 0:
            remaining = 'BUST'
        
        return score, remaining


    def _commit_score(self):
        self.scorer.commit_score([dart for dart in self.darts_in_visit if dart != ''])
        self.dart_coords_in_visit, self.darts_in_visit = [], ['']*3
        self.user_calibration = -np.ones((6, 2))
        self.wait_for_dart_removal = False
        self.pred_queue = -np.ones((5,3,2))
        self.pred_queue_count = 0


    def _adjust_coords(self, calibration_coords, dart_coords, resolution, crop_start, crop_size):
        # needed in order to adjust the coords for the square crop
        calibration_coords *= resolution # get pixel coords
        calibration_coords -= crop_start # adjust pixel coords for square crop
        calibration_coords /= crop_size # convert back to normalised coords
        if dart_coords.shape != (0,): # do same for darts
            dart_coords *= resolution
            dart_coords -= crop_start
            dart_coords /= crop_size
            dart_coords = dart_coords[np.all(np.logical_and(dart_coords>=0, dart_coords<=1), axis=1)] # remove any dart points detected outside of square crop
        
        return calibration_coords, dart_coords

    def _process_predictions(self, transformed_dart_coords, repeat_threshold):
        if len(transformed_dart_coords) == 0:
            self.pred_queue[self.pred_queue_count % 5] = -np.ones((3, 2))
        else:
            self.pred_queue[self.pred_queue_count % 5] = np.vstack((transformed_dart_coords, -np.ones((3-len(transformed_dart_coords), 2)))) # add [-1, -1] to fill any spaces when < 3 darts
        self.pred_queue_count += 1

        if self.wait_for_dart_removal:
            count = 0
            for frame in self.pred_queue:
                if np.all(frame == -1):
                    count += 1
            if count >= repeat_threshold:
                self._commit_score()
        
        elif self.darts_in_visit.count('') > 0:
            # check based on number of darts in visit and if the dart has been scored before
            unique_predictions = np.unique(self.pred_queue[self.pred_queue != -1].reshape(-1,2), axis=0)
            matches = {tuple(pred): [] for pred in unique_predictions} # for grouping together all similar predictions
            
            for frame in self.pred_queue:
                for pred in frame:
                    if np.any(pred == -1):
                        continue
                    for unique_pred in unique_predictions:
                        if self._distance(pred, unique_pred) < 0.01: # assume same prediction if distance < 0.01
                            matches[tuple(unique_pred)].append(pred)
                            break
            # sort dictionary based on length of values lists
            matches = {k: v for k, v in sorted(matches.items(), key=lambda item: len(item[1]), reverse=True) if len(v) >= repeat_threshold}
            best_predictions = []
            for _, match_ in matches.items():
                best_predictions.append(np.mean(match_, axis=0))
            
            if len(self.dart_coords_in_visit) == 0:
                self.dart_coords_in_visit = [pred for pred in best_predictions[:3]]
            
            else:
                for best_pred in best_predictions:
                    if all([self._distance(coords, best_pred) > 0.01 for coords in self.dart_coords_in_visit]):
                        if len(self.dart_coords_in_visit) == 3:
                            break
                        self.dart_coords_in_visit.append(best_pred)


    def _get_webcam_index(self):
        """
        Find the first available webcam index that works on both Windows and Linux
        """
        # Try common webcam indices
        for index in range(5):
            cap = cv2.VideoCapture(index)
            if cap.isOpened():
                # Test if we can read a frame
                ret, _ = cap.read()
                cap.release()
                if ret:
                    return index
        
        # If no webcam found, return 0 as default
        return 0

    def start(self, GUI, scorer, resolution:np.array):
        self.scorer = scorer
        self.num_corrections = 0
        
        crop_size=min(resolution)
        crop_start = resolution/2 - crop_size/2

        self.dart_coords_in_visit, self.darts_in_visit = [], ['']*3
        self.user_calibration = -np.ones((6, 2))
        self.wait_for_dart_removal = False
        self.game_over = False

        self.pred_queue = -np.ones((5,3,2)) # implement FIFO queue to store the last 5 frames' predictions
        self.pred_queue_count = 0
        repeat_threshold = 3 # threshold number of frames to commit a dart

        prev_frame_time = 0
        new_frame_time = 0

        # Setup webcam capture
        webcam_index = self._get_webcam_index()
        
        # On Windows, create VideoCapture with DirectShow backend for better compatibility
        if platform.system() == "Windows":
            cap = cv2.VideoCapture(webcam_index, cv2.CAP_DSHOW)
        else:
            cap = cv2.VideoCapture(webcam_index)
        
        # Set webcam properties for better performance
        cap.set(cv2.CAP_PROP_FRAME_WIDTH, int(resolution[1]))  # width
        cap.set(cv2.CAP_PROP_FRAME_HEIGHT, int(resolution[0]))  # height
        cap.set(cv2.CAP_PROP_FPS, 30)
        
        if not cap.isOpened():
            raise RuntimeError(f"Could not open webcam at index {webcam_index}")

        try:
            while True:
                if self.game_over:
                    break
                
                ret, frame = cap.read()
                if not ret:
                    print("Failed to read frame from webcam")
                    continue
                
                # Debug: Show raw camera feed (comment out when not debugging)
                cv2.imshow('Raw Camera Feed', frame)
                if cv2.waitKey(1) & 0xFF == ord('q'):
                    self.game_over = True
                    break
                
                # Convert frame to RGB (OpenCV uses BGR by default)
                frame_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
                
                # Run YOLO inference on the frame
                results = self.model(frame_rgb, verbose=True)
                
                for result in results:
                    calibration_coords, dart_coords = self.predict.process_yolo_output(result)
                    
                    # Debug: Print what we're detecting every 30 frames (1 second at 30 FPS)
                    if self.pred_queue_count % 30 == 0:
                        self._debug_detections(result, calibration_coords)
                    
                    if np.count_nonzero(calibration_coords == -1)/2 > 2:
                        continue
                    calibration_coords, dart_coords = self._adjust_coords(calibration_coords, dart_coords, resolution, crop_start, crop_size)
                    calibration_coords = np.where(self.user_calibration == -1, calibration_coords, self.user_calibration)

                    H_matrix = self.predict.find_homography(calibration_coords, crop_size)
                    transformed_dart_coords = self.predict.transform_to_boardplane(H_matrix[0], dart_coords, crop_size)
                    
                    self._process_predictions(transformed_dart_coords, repeat_threshold)
                    
                    self.darts_in_visit, score = self.predict.score(np.array(self.dart_coords_in_visit)) # must always run this in case user moves coords
                    while len(self.darts_in_visit) < 3:
                        self.darts_in_visit.append('')
                    
                    score, remaining = self._assess_visit(self.darts_in_visit)

                    new_frame_time = time.time()
                    fps = round(1/(new_frame_time - prev_frame_time), 1)
                    prev_frame_time = new_frame_time
                    
                    GUI._display_graphics(result, H_matrix, crop_start, crop_size, calibration_coords, dart_coords, score, remaining, fps)
                    
                    # Break after processing first result to maintain single frame processing
                    break

        finally:
            cap.release()
            cv2.destroyAllWindows()  # Clean up debug windows

        print(f'Number of user corrections: {self.num_corrections}')
        print(f'Number of darts thrown: {np.sum(self.scorer.num_dart_history)}')

if __name__ == "__main__":
    pass