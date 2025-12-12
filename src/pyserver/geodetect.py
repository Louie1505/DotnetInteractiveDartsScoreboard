import cv2
import numpy as np
import json
import os

class SimpleDartboardDetector:
    def __init__(self):
        self.dartboard_center = None
        self.dartboard_radius = None
        
        # Simple smoothing for stability
        self.last_detection = None
        self.detection_count = 0
        self.stable_detections = []  # Store last few good detections
        self.max_stable = 3
        
        # Settings file path
        self.settings_file = 'dartboard_settings.json'
        
        # Adjustable parameters for reflection handling (defaults)
        self.dark_threshold = 70  # Adjustable with arrow keys
        self.brightness_adjust = 0  # Brightness adjustment (-50 to +50)
        self.contrast_adjust = 1.0  # Contrast multiplier (0.5 to 2.0)
        
        # Load saved settings
        self.load_settings()
    
    def load_settings(self):
        """Load settings from JSON file"""
        try:
            if os.path.exists(self.settings_file):
                with open(self.settings_file, 'r') as f:
                    settings = json.load(f)
                    self.dark_threshold = settings.get('dark_threshold', 70)
                    self.brightness_adjust = settings.get('brightness_adjust', 0)
                    self.contrast_adjust = settings.get('contrast_adjust', 1.0)
                    print(f"Loaded settings: Threshold={self.dark_threshold}, Brightness={self.brightness_adjust}, Contrast={self.contrast_adjust:.2f}")
        except Exception as e:
            print(f"Error loading settings: {e}. Using defaults.")
    
    def save_settings(self):
        """Save current settings to JSON file"""
        try:
            settings = {
                'dark_threshold': self.dark_threshold,
                'brightness_adjust': self.brightness_adjust,
                'contrast_adjust': self.contrast_adjust
            }
            with open(self.settings_file, 'w') as f:
                json.dump(settings, f, indent=2)
        except Exception as e:
            print(f"Error saving settings: {e}")
        
    def detect_dartboard(self, frame):
        """
        Detect dartboard by finding the darkest circular area
        """
        # Convert to grayscale
        gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
        
        # Apply brightness and contrast adjustments
        adjusted = cv2.convertScaleAbs(gray, alpha=self.contrast_adjust, beta=self.brightness_adjust)
        
        # Better noise reduction
        blurred = cv2.GaussianBlur(adjusted, (7, 7), 1.5)  # Slightly larger blur
        blurred = cv2.bilateralFilter(blurred, 5, 50, 50)  # Edge-preserving smoothing
        
        # Create mask for dark areas (dartboard background)
        # Find the darkest areas in the image
        dark_mask = cv2.threshold(blurred, self.dark_threshold, 255, cv2.THRESH_BINARY_INV)[1]
        
        # More refined cleanup
        kernel_small = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (3, 3))
        kernel_large = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (9, 9))
        
        # Fill small gaps first
        dark_mask = cv2.morphologyEx(dark_mask, cv2.MORPH_CLOSE, kernel_small)
        # Remove noise
        dark_mask = cv2.morphologyEx(dark_mask, cv2.MORPH_OPEN, kernel_small)
        # Final smoothing with larger kernel
        dark_mask = cv2.morphologyEx(dark_mask, cv2.MORPH_CLOSE, kernel_large)
        
        # Final noise reduction
        dark_mask = cv2.medianBlur(dark_mask, 3)  # Smaller median filter
        
        # Find circles in the dark mask
        circles = cv2.HoughCircles(
            dark_mask,
            cv2.HOUGH_GRADIENT,
            dp=1,
            minDist=120,   # Increased to prevent multiple detections
            param1=15,     # Lower for binary mask
            param2=12,     # Even lower threshold for stability
            minRadius=85,  # Slightly tighter range
            maxRadius=175
        )
        
        if circles is not None:
            circles = np.round(circles[0, :]).astype("int")
            
            # Find the circle with the most dark pixels inside it
            best_circle = None
            best_dark_ratio = 0
            
            for (x, y, r) in circles:
                # Basic position validation
                if (r + 20 < x < frame.shape[1] - r - 20 and 
                    r + 20 < y < frame.shape[0] - r - 20):
                    
                    # Check how much of the circle area is dark
                    dark_ratio = self._calculate_dark_ratio(dark_mask, x, y, r)
                    
                    if dark_ratio > best_dark_ratio and dark_ratio > 0.25:  # Slightly lower threshold
                        best_dark_ratio = dark_ratio
                        best_circle = (x, y, r)
            
            if best_circle:
                x, y, r = best_circle
                
                # Enhanced smoothing with multiple detection history
                if self.last_detection is not None:
                    last_x, last_y, last_r = self.last_detection
                    
                    # Check if this is a reasonable change
                    center_diff = np.sqrt((x - last_x)**2 + (y - last_y)**2)
                    radius_diff = abs(r - last_r)
                    
                    # If changes are small, use stronger smoothing
                    if center_diff < 20 and radius_diff < 10:
                        # Add to stable detections
                        self.stable_detections.append((x, y, r))
                        if len(self.stable_detections) > self.max_stable:
                            self.stable_detections.pop(0)
                        
                        # Use averaged values for stability
                        if len(self.stable_detections) >= 2:
                            avg_x = int(np.mean([d[0] for d in self.stable_detections]))
                            avg_y = int(np.mean([d[1] for d in self.stable_detections]))
                            avg_r = int(np.mean([d[2] for d in self.stable_detections]))
                            x, y, r = avg_x, avg_y, avg_r
                    else:
                        # Large change - reset stable detections
                        self.stable_detections = [(x, y, r)]
                
                self.last_detection = (x, y, r)
                self.dartboard_center = (x, y)
                self.dartboard_radius = r
                self.detection_count += 1
                
                return (x, y), r
        
        return None, None
    
    def _calculate_dark_ratio(self, dark_mask, cx, cy, radius):
        """
        Calculate what percentage of the circle area is dark
        """
        # Create a circular mask
        mask = np.zeros(dark_mask.shape, dtype=np.uint8)
        cv2.circle(mask, (cx, cy), radius, 255, -1)
        
        # Count dark pixels within the circle
        circle_area = np.sum(mask > 0)
        dark_pixels_in_circle = np.sum((dark_mask > 0) & (mask > 0))
        
        if circle_area > 0:
            return dark_pixels_in_circle / circle_area
        return 0
    
    def draw_expected_dartboard_lines(self):
        """
        Generate the expected 20 radial lines for a standard dartboard
        Standard dartboard starts at 9 degrees, then every 18 degrees (360/20)
        """
        if not self.dartboard_center or not self.dartboard_radius:
            return []
        
        lines = []
        cx, cy = self.dartboard_center
        
        # Start at 9 degrees, then every 18 degrees for 20 segments
        for i in range(20):
            angle = (9 + i * 18) * np.pi / 180  # Convert to radians
            
            # Calculate end point on the dartboard edge
            end_x = int(cx + np.cos(angle) * (self.dartboard_radius - 10))
            end_y = int(cy + np.sin(angle) * (self.dartboard_radius - 10))
            
            lines.append(((cx, cy), (end_x, end_y)))
        
        return lines
    
    def draw_dartboard(self, frame):
        """
        Draw the detected dartboard circle
        """
        result_frame = frame.copy()
        
        if self.dartboard_center and self.dartboard_radius:
            # Draw the dartboard circle in green
            cv2.circle(result_frame, self.dartboard_center, self.dartboard_radius, (0, 255, 0), 3)
            # Draw center point
            cv2.circle(result_frame, self.dartboard_center, 5, (0, 255, 0), -1)
            
            # Draw expected radial lines (20 segments every 18 degrees)
            expected_lines = self.draw_expected_dartboard_lines()
            for line in expected_lines:
                (x1, y1), (x2, y2) = line
                cv2.line(result_frame, (x1, y1), (x2, y2), (255, 0, 255), 1)
            
            # Add text showing dartboard info
            text = f"Dartboard: R={self.dartboard_radius}, Segments=20"
            cv2.putText(result_frame, text, (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 255, 0), 2)
            
            # Show current adjustment values
            adj_text = f"T:{self.dark_threshold} B:{self.brightness_adjust} C:{self.contrast_adjust:.2f}"
            cv2.putText(result_frame, adj_text, (10, 60), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (255, 255, 0), 1)
        else:
            # Show searching status
            cv2.putText(result_frame, "Searching for dartboard...", (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 0, 255), 2)
            
            # Show current adjustment values even when searching
            adj_text = f"T:{self.dark_threshold} B:{self.brightness_adjust} C:{self.contrast_adjust:.2f}"
            cv2.putText(result_frame, adj_text, (10, 60), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (255, 255, 0), 1)
        
        # Draw keybind instructions
        keybinds = [
            "Controls:",
            "T/G = Threshold +/-",
            "Y/H = Brightness -/+", 
            "U/J = Contrast +/-",
            "R = Reset, D = Debug, Q = Quit"
        ]
        
        start_y = result_frame.shape[0] - 120  # Position from bottom
        for i, text in enumerate(keybinds):
            y_pos = start_y + (i * 22)
            if i == 0:  # Header
                cv2.putText(result_frame, text, (10, y_pos), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (255, 255, 255), 1)
            else:  # Instructions
                cv2.putText(result_frame, text, (10, y_pos), cv2.FONT_HERSHEY_SIMPLEX, 0.4, (255, 255, 255), 1)
        
        return result_frame
    
    def show_debug_mask(self, frame):
        """
        Show the dark mask for debugging
        """
        gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
        adjusted = cv2.convertScaleAbs(gray, alpha=self.contrast_adjust, beta=self.brightness_adjust)
        blurred = cv2.GaussianBlur(adjusted, (7, 7), 1.5)
        blurred = cv2.bilateralFilter(blurred, 5, 50, 50)
        
        # Create the same dark mask as in detection
        dark_mask = cv2.threshold(blurred, self.dark_threshold, 255, cv2.THRESH_BINARY_INV)[1]
        
        kernel_small = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (3, 3))
        kernel_large = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (9, 9))
        
        dark_mask = cv2.morphologyEx(dark_mask, cv2.MORPH_CLOSE, kernel_small)
        dark_mask = cv2.morphologyEx(dark_mask, cv2.MORPH_OPEN, kernel_small)
        dark_mask = cv2.morphologyEx(dark_mask, cv2.MORPH_CLOSE, kernel_large)
        dark_mask = cv2.medianBlur(dark_mask, 3)
        
        return dark_mask
    
    def adjust_threshold(self, delta):
        """Adjust dark threshold for reflection handling"""
        self.dark_threshold = max(0, min(255, self.dark_threshold + delta))
        print(f"Dark threshold: {self.dark_threshold}")
        self.save_settings()
    
    def adjust_brightness(self, delta):
        """Adjust brightness for reflection handling"""
        self.brightness_adjust = max(-50, min(50, self.brightness_adjust + delta))
        print(f"Brightness adjust: {self.brightness_adjust}")
        self.save_settings()
    
    def adjust_contrast(self, delta):
        """Adjust contrast for reflection handling"""
        self.contrast_adjust = max(0.5, min(2.0, self.contrast_adjust + delta))
        print(f"Contrast adjust: {self.contrast_adjust:.2f}")
        self.save_settings()
    
    def reset_adjustments(self):
        """Reset all adjustments to defaults"""
        self.dark_threshold = 70
        self.brightness_adjust = 0
        self.contrast_adjust = 1.0
        print("Reset to defaults: Threshold=70, Brightness=0, Contrast=1.0")
        self.save_settings()

if __name__ == "__main__":
    # Test the detector
    detector = SimpleDartboardDetector()
    
    # Initialize webcam
    cap = cv2.VideoCapture(0)
    
    print("Simple Dartboard Detector")
    print("Press 'q' to quit, 'd' to toggle debug mask")
    print("Controls: W/S = threshold, A/Z = brightness, E/X = contrast")
    print("'r' = reset adjustments")
    
    show_debug = False
    
    try:
        while True:
            ret, frame = cap.read()
            if not ret:
                continue
            
            # Detect dartboard
            center, radius = detector.detect_dartboard(frame)
            
            # Draw result
            result_frame = detector.draw_dartboard(frame)
            
            # Show debug mask if requested
            if show_debug:
                debug_mask = detector.show_debug_mask(frame)
                cv2.imshow('Dark Mask Debug', debug_mask)
            
            # Show frame
            cv2.imshow('Dartboard Detection', result_frame)
            
            # Check for quit or debug toggle
            key = cv2.waitKey(1) & 0xFF
            if key == ord('q'):
                break
            elif key == ord('d'):
                show_debug = not show_debug
                if not show_debug:
                    cv2.destroyWindow('Dark Mask Debug')
                print(f"Debug mask: {'ON' if show_debug else 'OFF'}")
            elif key == ord('r'):
                detector.reset_adjustments()
            # Use letter keys for easier input
            elif key == ord('t'):  # T - increase threshold
                detector.adjust_threshold(5)
            elif key == ord('g'):  # G - decrease threshold
                detector.adjust_threshold(-5)
            elif key == ord('y'):  # Y - increase brightness
                detector.adjust_brightness(5)
            elif key == ord('h'):  # H - decrease brightness
                detector.adjust_brightness(-5)
            elif key == ord('u'):  # U - increase contrast
                detector.adjust_contrast(0.1)
            elif key == ord('J'):  # j - decrease contrast
                detector.adjust_contrast(-0.1)
    
    finally:
        cap.release()
        cv2.destroyAllWindows()
