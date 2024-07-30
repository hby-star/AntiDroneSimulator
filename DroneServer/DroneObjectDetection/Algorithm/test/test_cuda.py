import torch


def check_cuda_availability():
    print(f"PyTorch version: {torch.__version__}")
    if torch.cuda.is_available():
        print("CUDA is available. PyTorch can use GPU.")
        print(f"CUDA version: {torch.version.cuda}")
        print(f"Number of GPUs: {torch.cuda.device_count()}")
        print(f"Current GPU: {torch.cuda.current_device()}")
        print(f"GPU Name: {torch.cuda.get_device_name(torch.cuda.current_device())}")
    else:
        print("CUDA is not available. PyTorch is using CPU.")


check_cuda_availability()
