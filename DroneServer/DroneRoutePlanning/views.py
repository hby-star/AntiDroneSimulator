import argparse
import json
import os

import numpy as np
from PIL import Image
from django.http import JsonResponse
from drf_yasg import openapi
from drf_yasg.utils import swagger_auto_schema
from rest_framework import status
from rest_framework.decorators import api_view

from DroneObjectDetection.Algorithm.yolo import yolo
from DroneRoutePlanning.D3QN.D3QN import D3QN, d3qn_select_action
from DroneRoutePlanning.DQN.DQN import dqn_select_action
from DroneRoutePlanning.DroneEnvironment.DroneEnvironment import drone_move_directions, DroneEnvironment

save_count = 0


@swagger_auto_schema(
    operation_summary="Drone Route Planning",
    tags=['Drone Route Planning'],
    methods=['POST'],
    request_body=openapi.Schema(
        type=openapi.TYPE_OBJECT,
        properties={
            'drone_image': openapi.Schema(type=openapi.TYPE_FILE, description='无人机拍摄的图像'),
            'obstacle_positions': openapi.Schema(
                type=openapi.TYPE_ARRAY,
                items=openapi.Schema(type=openapi.TYPE_NUMBER),
                description='障碍物的位置'
            ),
        }
    ),
    responses={
        200: openapi.Schema(
            type=openapi.TYPE_OBJECT,
            properties={
                'output': openapi.Schema(type=openapi.TYPE_STRING, description='Output of the route planning model')
            }
        ),
        400: openapi.Schema(
            type=openapi.TYPE_OBJECT,
            properties={
                'error': openapi.Schema(type=openapi.TYPE_STRING, description='Error message')
            }
        ),
        500: openapi.Schema(
            type=openapi.TYPE_OBJECT,
            properties={
                'error': openapi.Schema(type=openapi.TYPE_STRING, description='Internal server error')
            }
        )
    }
)
@api_view(['POST'])
def drone_route_planning(request):
    global save_count
    try:
        # Get the uploaded file
        if ('drone_image' not in request.FILES or
                'obstacle_positions' not in request.data):
            return JsonResponse({'error': 'Missing required parameters'}, status=status.HTTP_400_BAD_REQUEST)

        uploaded_file = request.FILES['drone_image']

        # Convert the uploaded file to a PIL image
        input_image = Image.open(uploaded_file)

        # Run the YOLO model
        results = yolo(input_image)
        if results:
            res_json = results[0].tojson()
        else:
            return JsonResponse({'No person detected': '[]'})

        # get person position in camera from yolo results
        res_list = json.loads(res_json)
        person_position_in_camera = []
        for res_object in res_list:
            if res_object['name'] == 'person':
                person_position_in_camera = [res_object['box']['x1'], res_object['box']['y1'], res_object['box']['x2'],
                                             res_object['box']['y2']]
                break

        if not person_position_in_camera:
            return JsonResponse({'No person detected': '[]'})

        obstacle_positions = json.loads(request.data['obstacle_positions'])

        state = np.concatenate(
            [np.array(person_position_in_camera), np.array(obstacle_positions)])

        action = d3qn_select_action(state)

        # Save the input and output to save.md
        save_count += 1
        save_input_output(action, obstacle_positions, input_image, save_count)

        return JsonResponse({'Direction': drone_move_directions[action].tolist()})
    except Exception as e:
        return JsonResponse({'error': str(e)}, status=status.HTTP_500_INTERNAL_SERVER_ERROR)


def save_input_output(action, obstacle_positions, input_image, save_count):
    save_dir = './DroneRoutePlanning'
    save_path = os.path.join(save_dir, 'save.md')
    image_dir = os.path.join(save_dir, 'image')
    os.makedirs(image_dir, exist_ok=True)

    image_file_path = os.path.join(image_dir, f"drone_image_{save_count}.jpg")
    input_image.save(image_file_path)

    with open(save_path, 'a', encoding='utf-8') as f:
        print(f"Saving input and output to {save_path}")
        f.write(f"##Record {save_count}\n")
        f.write("### 输入\n")
        f.write(f"**无人机图像:** ![drone_image]({image_file_path})\n")
        f.write(f"**障碍物位置:** {obstacle_positions}\n")
        f.write("\n### 输出\n")
        f.write(f"**方向:** {drone_move_directions[action].tolist()}\n")
