# Real-Time Facial Expression Recognition (LBP, HOG, mini-Xception)

This project implements a **real-time Facial Expression Recognition system** using three different models:

- **LBP + KNN** (classical baseline)
- **HOG + Linear SVM** (classical feature-based model)
- **mini-Xception (CNN)** pretrained on FER-2013

The goal is to compare:
- **latency**
- **(optional) tiny accuracy**
- **real-time behavior using a webcam interface**

This is an official Lab assignment for the ENSTA's class of Social Robotics.

---

# Project Structure

```
TP2-RealTimeDetection/
│
├── interface.py                  # Gradio real-time interface
├── benchmark_fer.py              # Script to measure latency + tiny accuracy
├── utils.py                      # face detection + latency helper
│
├── models/
│   ├── lbp_knn.py                # LBP + artificial KNN
│   ├── hog_svm.py                # HOG + artificial SVM
│   ├── mini_xception.py          # mini-Xception loader (compile=False)
│   ├── haarcascade_frontalface_default.xml
│   └── mini_xception_weights.h5  # pretrained CNN weights (FER-2013)
│
├── train/                        # FER2013 train split (images)
└── test/                         # FER2013 test split (images)
```

---

# To run the project yourself: 

Create and activate your environment:

```bash
conda create -n fer python=3.10
conda activate fer
```

Install dependencies:

```bash
pip install -r requirements.txt
```

Note: this requirements.txt will include several "unecessary" libraries, as it is my personal environment for machine-learning.

---

# Running the Real-Time Interface

Simply run:

```bash
python interface.py
```

Gradio will open a local URL:

```
http://127.0.0.1:7860
```

The page will show:

- your webcam stream  
- 3 text boxes updating in real-time  
  - **LBP + KNN**  
  - **HOG + SVM**  
  - **mini-Xception (CNN)**  

For each frame, you see:

- predicted emotion  
- confidence  
- latency (ms per inference)

---

# Benchmark: Latency + Tiny Accuracy 

The Lab requires:

> “One mini-table with mean latency per model (+ optional tiny accuracy on 20–50 test images).”

There is a ready-to-run script:

```bash
python benchmark_fer.py
```

This script:

- samples a small number of images from the `test/` folders (of the FER dataset)
- runs all three models  
- measures average latency  
- computes a tiny accuracy over ~35 images  

Example output of when I used N_IMAGES_PER_CLASS = 5 (even though when I used = 100 it did not change much):

```
===== LATENCY (ms) =====
LBP + KNN      : 0.66 ms
HOG + SVM      : 0.41 ms
Mini-Xception  : 20.43 ms

===== ACCURACY (optional) =====
Total images used: 35
LBP  : 5/35 = 0.14
HOG  : 6/35 = 0.17
CNN  : 16/35 = 0.46
```

---

# Model Details

### ✔ mini-Xception (CNN)
- Fully pretrained on FER-2013  
- Input: **64×64 grayscale**  
- Output: 7 emotions  
- Loaded with:
  ```python
  load_model(path, compile=False)
  ```

### ✔ LBP + KNN (classical baseline)
- Uses OpenCV LBP-like feature (Laplacian-based)  
- KNN is **artificial**, not trained  
- Extremely fast (<1 ms)

### ✔ HOG + SVM (classical baseline)
- Uses HOG (skimage)  
- SVM is **artificial** (random weights)  
- Very fast (<1 ms)

---

# Interesting question: why did the KNN and SVM models ran poorly? 

- They are **not trained**, intentionally (as per lab instructions)
- LBP + KNN lacks discriminative power  
- HOG + SVM without training barely understands emotion geometry  
- The goal is to show the **huge accuracy gap** with CNNs

But they are:

- extremely fast → perfect for comparison  
- educational, as it serves the purpose of this lab by Adriana and Juan → demonstrate feature engineering  


