import json

import numpy as np
from PIL import Image
from django.http import JsonResponse
from drf_yasg import openapi
from drf_yasg.utils import swagger_auto_schema
from rest_framework import status
from rest_framework.decorators import api_view

from DroneObjectDetection.Algorithm.yolo import yolo
from DroneRoutePlanning.Algorithm.DQN import dqn_select_action
from DroneRoutePlanning.Algorithm.DroneEnvironment import drone_move_directions


@swagger_auto_schema(
    operation_summary="Drone Route Planning",
    tags=['Drone Route Planning'],
    methods=['POST'],
    request_body=openapi.Schema(
        type=openapi.TYPE_OBJECT,
        properties={
            'drone_image': openapi.Schema(type=openapi.TYPE_FILE, description='无人机拍摄的图像'),
            'drone_position': openapi.Schema(
                type=openapi.TYPE_ARRAY,
                items=openapi.Schema(type=openapi.TYPE_NUMBER),
                description='无人机的位置'
            ),
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
    try:
        # Get the uploaded file
        if ('drone_image' not in request.FILES or
                'drone_position' not in request.data or
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

        drone_position = json.loads(request.data['drone_position'])
        obstacle_positions = json.loads(request.data['obstacle_positions'])

        state = np.concatenate(
            [np.array(drone_position), np.array(person_position_in_camera), np.array(obstacle_positions)])

        action = dqn_select_action(state)

        return JsonResponse({'Direction': drone_move_directions[action].tolist()})
    except Exception as e:
        return JsonResponse({'error': str(e)}, status=status.HTTP_500_INTERNAL_SERVER_ERROR)
