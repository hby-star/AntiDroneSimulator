import torch
from PIL import Image

# 加载模型
model = torch.hub.load('ultralytics/yolov5', 'yolov5s')  # 加载 YOLOv5s 模型

# 加载图像
img = Image.open('images/img.png')

# 推理
results = model(img)

# 显示结果
results.show()

# 或者保存结果
results.save(save_dir='results/')  # 将结果保存到 'results/' 目录
