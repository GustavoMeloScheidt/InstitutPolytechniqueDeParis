import numpy as np
import cv2
from tensorflow.keras.models import load_model
import os

class MiniXceptionFER:
    def __init__(self, weights_path=None):
        if weights_path is None:
            weights_path = os.path.join(
                os.path.dirname(__file__),
                "mini_xception_weights.h5"
            )

        if not os.path.exists(weights_path):
            raise FileNotFoundError(
                f"Arquivo de pesos não encontrado em: {weights_path}"
            )

        # importante: compile=False para ignorar optimizer antigo (lr, decay, etc.)
        self.model = load_model(weights_path, compile=False)

        self.class_names = [
            "Angry", "Disgust", "Fear", "Happy",
            "Sad", "Surprise", "Neutral"
        ]

    def preprocess(self, face_bgr):
        # modelo do repo atual espera 64x64x1
        gray = cv2.cvtColor(face_bgr, cv2.COLOR_BGR2GRAY)
        gray = cv2.resize(gray, (64, 64))      # <-- 64,64 (não 48,48)
        gray = gray.astype("float32") / 255.0
        # shape final: (1, 64, 64, 1)
        gray = np.expand_dims(gray, axis=-1)
        gray = np.expand_dims(gray, axis=0)
        return gray

    def predict(self, face_bgr):
        x = self.preprocess(face_bgr)
        probs = self.model.predict(x, verbose=0)[0]
        idx = int(np.argmax(probs))
        return self.class_names[idx], float(probs[idx])
