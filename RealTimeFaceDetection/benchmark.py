import os
import time
import random

import cv2
import numpy as np

from models.lbp_knn import LBP_KNN
from models.hog_svm import HOG_SVM
from models.mini_xception import MiniXceptionFER
from utils import detect_face

# ===============================
# CONFIGURATION
# ===============================

# Path the folder with FER2013 images 
DATA_ROOT = "dataset/test"

# Number of images to sample from each class
N_IMAGES_PER_CLASS = 100

# FER2013 emotion labels
CLASS_NAMES = ["angry", "disgust", "fear", "happy", "neutral", "sad", "surprise"]

# ===============================


# Load the three models (same ones used in the Gradio interface)
lbp_model = LBP_KNN()
hog_model = HOG_SVM()
cnn_model = MiniXceptionFER()


def sample_images():
    """
    Randomly selects up to N_IMAGES_PER_CLASS images from each emotion folder.
    Returns a list of (true_label, filepath).
    """
    samples = []

    for cls in CLASS_NAMES:
        folder = os.path.join(DATA_ROOT, cls)
        if not os.path.isdir(folder):
            print(f"Directory not found: {folder}")
            continue

        files = [
            f for f in os.listdir(folder)
            if f.lower().endswith((".jpg", ".png", ".jpeg"))
        ]

        if not files:
            continue

        random.shuffle(files)
        chosen = files[:N_IMAGES_PER_CLASS]

        for fname in chosen:
            samples.append((cls, os.path.join(folder, fname)))

    return samples


def measure_latency(model_fn, img, repeats=5):
    """
    Measures average latency (ms) by running the model several times.
    """
    times = []
    for _ in range(repeats):
        t0 = time.time()
        _ = model_fn(img)
        times.append((time.time() - t0) * 1000.0)
    return float(np.mean(times))


def run_benchmark():
    samples = sample_images()
    if not samples:
        print("No images found. Check DATA_ROOT path.")
        return

    lat_lbp, lat_hog, lat_cnn = [], [], []
    correct_lbp = correct_hog = correct_cnn = 0
    total = 0

    for true_label, path in samples:
        img = cv2.imread(path)
        if img is None:
            print("Failed to load image:", path)
            continue

        # Try face detection; if none found, use the full image (FER is already cropped)
        face = detect_face(img)
        if face is None:
            face = img

        total += 1

        # LATENCY
        lat_lbp.append(measure_latency(lbp_model.predict, face))
        lat_hog.append(measure_latency(hog_model.predict, face))
        lat_cnn.append(measure_latency(cnn_model.predict, face))

        # PREDICTIONS (for optional accuracy)
        pred_lbp, _ = lbp_model.predict(face)
        pred_hog, _ = hog_model.predict(face)
        pred_cnn, _ = cnn_model.predict(face)

        true_label = true_label.lower()

        if pred_lbp.lower().startswith(true_label):
            correct_lbp += 1

        if pred_hog.lower().startswith(true_label):
            correct_hog += 1

        if pred_cnn.lower().startswith(true_label):
            correct_cnn += 1

    # ===============================
    # PRINT RESULTS
    # ===============================

    print("\n===== LATENCY (ms) =====")
    print(f"LBP + KNN      : {np.mean(lat_lbp):.2f} ms")
    print(f"HOG + SVM      : {np.mean(lat_hog):.2f} ms")
    print(f"Mini-Xception  : {np.mean(lat_cnn):.2f} ms")

    print("\n===== ACCURACY (optional) =====")
    if total > 0:
        print(f"Total images used: {total}")
        print(f"LBP  : {correct_lbp}/{total} = {correct_lbp/total:.2f}")
        print(f"HOG  : {correct_hog}/{total} = {correct_hog/total:.2f}")
        print(f"CNN  : {correct_cnn}/{total} = {correct_cnn/total:.2f}")


if __name__ == "__main__":
    run_benchmark()
