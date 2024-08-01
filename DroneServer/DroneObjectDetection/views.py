from PIL import Image
from django.http import JsonResponse
from drf_yasg import openapi
from drf_yasg.utils import swagger_auto_schema
from rest_framework.decorators import api_view
from rest_framework import status

from DroneObjectDetection.Algorithm.yolo import yolo


@swagger_auto_schema(
    operation_summary="Drone Object Detection",
    tags=['Drone Object Detection'],
    methods=['POST'],
    request_body=openapi.Schema(
        type=openapi.TYPE_OBJECT,
        properties={
            'image': openapi.Schema(type=openapi.TYPE_FILE, description='Image file to be processed')
        }
    ),
    responses={
        200: openapi.Schema(
            type=openapi.TYPE_OBJECT,
            properties={
                'output': openapi.Schema(type=openapi.TYPE_STRING, description='Output of the object detection model')
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
def drone_object_detection(request):
    try:
        # Get the uploaded file
        if 'image' not in request.FILES:
            return JsonResponse({'error': 'No image file uploaded'}, status=status.HTTP_400_BAD_REQUEST)

        uploaded_file = request.FILES['image']

        # Convert the uploaded file to a PIL image
        input_image = Image.open(uploaded_file)

        # Run the YOLO model
        results = yolo(input_image)

        res_json = results[0].tojson()

        return JsonResponse({'output': res_json})
    except Exception as e:
        return JsonResponse({'error': str(e)}, status=status.HTTP_500_INTERNAL_SERVER_ERROR)
