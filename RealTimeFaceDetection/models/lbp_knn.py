import cv2
import numpy as np
from sklearn.neighbors import KNeighborsClassifier

class LBP_KNN:
    def __init__(self):
        """
        Modelo LBP+KNN artificial sem treino prévio.
        Perfeito para demonstração de latência no LAB.
        """
        rng = np.random.RandomState(42)
        self.class_names = [
            "Angry", "Disgust", "Fear", "Happy",
            "Sad", "Surprise", "Neutral"
        ]

        # Criamos 7 centróides artificiais de 100 features
        self.X = rng.rand(7, 100)
        self.y = np.array(self.class_names)

        self.knn = KNeighborsClassifier(n_neighbors=1)
        self.knn.fit(self.X, self.y)

    def _lbp_features(self, face_bgr, size=48):
        gray = cv2.cvtColor(face_bgr, cv2.COLOR_BGR2GRAY)
        gray = cv2.resize(gray, (size, size))
        lbp = cv2.Laplacian(gray, cv2.CV_8U)
        return lbp.flatten()[:100].astype("float32")

    def predict(self, face_bgr):
        feat = self._lbp_features(face_bgr).reshape(1, -1)
        pred = self.knn.predict(feat)[0]

        dist, _ = self.knn.kneighbors(feat)
        conf = 1 / (1 + dist[0][0])

        return pred, float(conf)
