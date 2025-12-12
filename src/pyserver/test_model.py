from ultralytics import YOLO
import cv2
import platform

class ModelTester:
    def __init__(self, weights_path="runs/detect/dartboard_detection/weights/best.pt"):
        self.model = YOLO(weights_path)
        self.class_names = {0: '20', 1: '3', 2: '11', 3: '6', 4: 'dart'}
    
    def _get_webcam_index(self):
        """Find the first available webcam index"""
        for index in range(5):
            cap = cv2.VideoCapture(index)
            if cap.isOpened():
                ret, _ = cap.read()
                cap.release()
                if ret:
                    return index
        return 0
    
    def test_live(self):
        """Test the model with live webcam feed"""
        webcam_index = self._get_webcam_index()
        
        if platform.system() == "Windows":
            cap = cv2.VideoCapture(webcam_index, cv2.CAP_DSHOW)
        else:
            cap = cv2.VideoCapture(webcam_index)
        
        if not cap.isOpened():
            raise RuntimeError(f"Could not open webcam at index {webcam_index}")
        
        print("Testing model - Press 'q' to quit")
        
        try:
            while True:
                ret, frame = cap.read()
                if not ret:
                    continue
                
                # Run inference
                results = self.model(frame, verbose=False)
                
                # Draw results on frame
                annotated_frame = results[0].plot()
                
                # Add detection info
                if len(results[0].boxes) > 0:
                    classes = results[0].boxes.cls
                    conf = results[0].boxes.conf
                    
                    for i, (cls, confidence) in enumerate(zip(classes, conf)):
                        class_name = self.class_names.get(int(cls.item()), 'unknown')
                        cv2.putText(annotated_frame, f"{class_name}: {confidence:.2f}", 
                                   (10, 30 + i*30), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 255, 0), 2)
                
                cv2.imshow('Model Test', annotated_frame)
                
                if cv2.waitKey(1) & 0xFF == ord('q'):
                    break
                    
        finally:
            cap.release()
            cv2.destroyAllWindows()

if __name__ == "__main__":
    tester = ModelTester()
    tester.test_live()