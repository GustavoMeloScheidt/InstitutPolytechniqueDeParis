import cv2
import numpy as np
from skimage.feature import hog
from sklearn.svm import LinearSVC

class HOG_SVM:
    def __init__(self):
        """
        Modelo HOG + SVM artificial sem treino.
        Apenas para demonstrar extração clássica + latência.
        """
        rng = np.random.RandomState(123)

        self.class_names = [
            "Angry", "Disgust", "Fear", "Happy",
            "Sad", "Surprise", "Neutral"
        ]

        n_classes = len(self.class_names)
        self.n_features = 500

        self.svm = LinearSVC()
        self.svm.classes_ = np.array(self.class_names)

        # pesos artificiais
        self.svm.coef_ = rng.randn(n_classes, self.n_features)
        self.svm.intercept_ = rng.randn(n_classes)

    def _hog_features(self, face_bgr, size=64):
        gray = cv2.cvtColor(face_bgr, cv2.COLOR_BGR2GRAY)
        gray = cv2.resize(gray, (size, size))

        feat = hog(
            gray,
            orientations=9,
            pixels_per_cell=(8, 8),
            cells_per_block=(2, 2),
            block_norm="L2-Hys",
            transform_sqrt=True
        ).astype("float32")

        if len(feat) >= self.n_features:
            feat = feat[:self.n_features]
        else:
            feat = np.pad(feat, (0, self.n_features - len(feat)), mode="constant")

        return feat

    def predict(self, face_bgr):
        feat = self._hog_features(face_bgr).reshape(1, -1)
        scores = self.svm.coef_.dot(feat.T).reshape(-1) + self.svm.intercept_

        idx = int(np.argmax(scores))
        label = self.class_names[idx]
        conf = float(scores[idx] / (np.sum(np.abs(scores)) + 1e-8))

        return label, conf
