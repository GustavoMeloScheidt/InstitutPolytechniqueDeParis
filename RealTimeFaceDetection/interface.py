import cv2
import numpy as np
import gradio as gr

from models.lbp_knn import LBP_KNN
from models.hog_svm import HOG_SVM
from models.mini_xception import MiniXceptionFER
from utils import detect_face, measure_latency

lbp_model = LBP_KNN()
hog_model = HOG_SVM()
cnn_model = MiniXceptionFER()

def process(frame):
    if frame is None:
        return "No frame", "No frame", "No frame"

    bgr = cv2.cvtColor(frame, cv2.COLOR_RGB2BGR)
    face = detect_face(bgr)

    if face is None:
        return "Face not found", "Face not found", "Face not found"

    lbp = measure_latency(lbp_model.predict, face)
    hog = measure_latency(hog_model.predict, face)
    cnn = measure_latency(cnn_model.predict, face)

    lbp_text = f"{lbp[0]} | conf={lbp[1]:.3f} | {lbp[2]} ms"
    hog_text = f"{hog[0]} | conf={hog[1]:.3f} | {hog[2]} ms"
    cnn_text = f"{cnn[0]} | conf={cnn[1]:.3f} | {cnn[2]} ms"

    return lbp_text, hog_text, cnn_text

with gr.Blocks() as demo:
    gr.Markdown("# Facial Expression Recognition – LBP vs HOG vs mini-Xception")

    cam = gr.Image(sources=["webcam"], streaming=True, label="Webcam")

    out_lbp = gr.Textbox(label="LBP + KNN")
    out_hog = gr.Textbox(label="HOG + SVM")
    out_cnn = gr.Textbox(label="mini-Xception (CNN)")

    cam.stream(process, inputs=cam, outputs=[out_lbp, out_hog, out_cnn])

demo.launch()
