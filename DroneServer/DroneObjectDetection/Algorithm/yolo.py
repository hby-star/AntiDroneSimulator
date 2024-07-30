from PIL import Image
from ultralytics import YOLO

model_path = "models/yolov8n.pt"
image_path = "images/test.png"

model = YOLO(model_path)


def yolo(input_image):
    results = model(input_image)

    return results[0]


if __name__ == '__main__':
    input_image = Image.open(image_path)
    result = yolo(input_image)
    result[0].show()
    print(type(result[0]))
