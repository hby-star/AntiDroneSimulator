import torch
import torch.nn as nn
import numpy as np
from django.http import JsonResponse


class DQN(nn.Module):
    def __init__(self, state_size, action_size):
        super(DQN, self).__init__()
        self.fc1 = nn.Linear(state_size, 128)
        self.fc2 = nn.Linear(128, 128)
        self.fc3 = nn.Linear(128, action_size)

    def forward(self, x):
        x = torch.relu(self.fc1(x))
        x = torch.relu(self.fc2(x))
        return self.fc3(x)


# 加载训练好的模型
# model_path = 'model.pth'
# state_size = 10
# action_size = 14
# model = DQN(state_size, action_size)
# model.load_state_dict(torch.load(model_path))
# model.eval()


# 预测动作方向
def predict_action(state):
    # state = torch.FloatTensor(state).unsqueeze(0)
    # with torch.no_grad():
    #     action_values = model(state)
    # action = np.argmax(action_values.numpy())
    # return action
    pass
