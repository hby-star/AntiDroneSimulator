import os
import random
from collections import namedtuple, deque

import torch
import torch.nn as nn
import torch.nn.functional as F

Transition = namedtuple('Transition',
                        ('state', 'action', 'next_state', 'reward'))


class ReplayMemory(object):

    def __init__(self, capacity):
        self.memory = deque([], maxlen=capacity)

    def push(self, *args):
        """Save a transition"""
        self.memory.append(Transition(*args))

    def sample(self, batch_size):
        return random.sample(self.memory, batch_size)

    def __len__(self):
        return len(self.memory)


class DQN(nn.Module):

    def __init__(self, n_observations, n_actions):
        super(DQN, self).__init__()
        self.layer1 = nn.Linear(n_observations, 128)
        self.layer2 = nn.Linear(128, 128)
        self.layer3 = nn.Linear(128, n_actions)

    # Called with either one element to determine next action, or a batch
    # during optimization. Returns tensor([[left0exp,right0exp]...]).
    def forward(self, x):
        x = F.relu(self.layer1(x))
        x = F.relu(self.layer2(x))
        return self.layer3(x)


BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
model_path = os.path.join(BASE_DIR, 'Algorithm', 'models', 'sim_reset.pth')
device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
dqn_model = DQN(10, 15).to(device)

dqn_model.load_state_dict(torch.load(model_path, weights_only=True))
dqn_model.eval()


def dqn_select_action(state):
    state = torch.tensor(state, dtype=torch.float32, device=device).unsqueeze(0)
    action = dqn_model(state).max(1).indices.view(1, 1)
    return action
