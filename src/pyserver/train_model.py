from ultralytics import YOLO
import yaml
import os

class DartboardTrainer:
    def __init__(self, data_dir="training_data"):
        self.data_dir = data_dir
        
    def create_dataset_yaml(self):
        """Create dataset configuration file"""
        dataset_config = {
            'path': os.path.abspath(self.data_dir),
            'train': 'images/train',
            'val': 'images/val',
            'test': 'images/test',
            'nc': 5,  # number of classes
            'names': ['20', '3', '11', '6', 'dart']
        }
        
        yaml_path = os.path.join(self.data_dir, 'dataset.yaml')
        with open(yaml_path, 'w') as f:
            yaml.dump(dataset_config, f, default_flow_style=False)
        
        print(f"Created dataset config: {yaml_path}")
        return yaml_path
    
    def train_model(self, epochs=100, img_size=640, batch_size=16):
        """Train the YOLO model"""
        # Create dataset config
        yaml_path = self.create_dataset_yaml()
        
        # Load a pretrained YOLOv8 model (nano version for speed)
        model = YOLO('yolov8n.pt')
        
        # Train the model
        results = model.train(
            data=yaml_path,
            epochs=epochs,
            imgsz=img_size,
            batch=batch_size,
            name='dartboard_detection',
            save=True,
            plots=True,
            device='auto'  # Use GPU if available
        )
        
        print("Training completed!")
        print(f"Best weights saved to: runs/detect/dartboard_detection/weights/best.pt")
        
        return results
    
    def validate_model(self, weights_path):
        """Validate the trained model"""
        model = YOLO(weights_path)
        
        # Run validation
        results = model.val()
        
        print("Validation results:")
        print(f"mAP50: {results.box.map50}")
        print(f"mAP50-95: {results.box.map}")
        
        return results

if __name__ == "__main__":
    trainer = DartboardTrainer("training_data")
    
    print("Starting training...")
    print("Make sure your training_data folder has this structure:")
    print("training_data/")
    print("  images/")
    print("    train/")
    print("    val/")
    print("  labels/")
    print("    train/")
    print("    val/")
    
    # Train the model
    trainer.train_model(epochs=50, batch_size=8)  # Reduced for faster training