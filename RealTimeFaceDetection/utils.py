import cv2
import time
import os

CASCADE_PATH = os.path.join("models", "haarcascade_frontalface_default.xml")
face_cascade = cv2.CascadeClassifier(CASCADE_PATH)

def detect_face(bgr):
    gray = cv2.cvtColor(bgr, cv2.COLOR_BGR2GRAY)
    faces = face_cascade.detectMultiScale(
        gray, scaleFactor=1.1, minNeighbors=4, minSize=(40, 40)
    )
    if len(faces) == 0:
        return None
    x, y, w, h = faces[0]
    return bgr[y:y+h, x:x+w]

def measure_latency(fn, img):
    t0 = time.time()
    label, conf = fn(img)
    dt = (time.time() - t0) * 1000
    return label, conf, round(dt, 2)
