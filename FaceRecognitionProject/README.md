# Facial Expression Classification with LBP and KNN

This repository contains the code for **Report 1 – Facial Expression Classification** of the *Social Robotics* course at **ENSTA**.

The goal is to classify facial expressions from static face images using a **classical computer vision pipeline** based on:

- **Local Binary Patterns (LBP)** for feature extraction  
- **K-Nearest Neighbors (KNN)** for classification  

The project uses a folder-based version of the **FER‑2013** dataset containing 48×48 grayscale facial images in seven emotion classes: **Angry, Disgust, Fear, Happy, Sad, Surprise, Neutral**.

---

## Repository structure

```text
.
├── dataset/
│   ├── train/
│   │   ├── angry/
│   │   ├── disgust/
│   │   ├── fear/
│   │   ├── happy/
│   │   ├── sad/
│   │   ├── surprise/
│   │   └── neutral/
│   └── test/
│       ├── angry/
│       ├── disgust/
│       ├── fear/
│       ├── happy/
│       ├── sad/
│       ├── surprise/
│       └── neutral/
├── knn_recognition.ipynb
└── README.md
```

- **`dataset/`** – training and test images organised by emotion label  
- **`knn_recognition.ipynb`** – Jupyter notebook implementing the full LBP + KNN pipeline  

> The dataset is **not** included in this repository by default.  
> You must download or place the FER‑2013 images in the `dataset/train` and `dataset/test` folders following the structure above.

---

## Methodology

The classification pipeline is composed of three main stages: 

1. **Preprocessing**
   - Convert images to grayscale (if needed)  
   - Resize to **48×48** pixels  
   - Normalize intensities to `uint8`  

2. **Feature Extraction – Local Binary Patterns (LBP)**
   - Uniform LBP with:
     - Number of neighbors: **P = 8**
     - Radius: **R = 1**
   - Image divided into a **4×4** grid of spatial blocks  
   - For each block, compute a **normalized histogram** of LBP codes  
   - Concatenate the 16 histograms into a single feature vector of length **160**  

3. **Classification – KNN**
   - Classifier: **KNeighborsClassifier** from `scikit-learn`  
   - Number of neighbors: **k = 5**  
   - Distance metric: **Euclidean**  
   - Train on LBP features from the training set and evaluate on the test set  

---

## Results

On the held‑out test set, the model obtained: fileciteturn2file0

- **Test accuracy:** ~**31%**

The class-wise results show heterogeneous performance:

- **Happy, Surprise, Disgust** – higher precision and recall  
- **Neutral, Fear, Sad** – more difficult, with lower recall and F1 score  

These outcomes are consistent with classical FER baselines using hand‑crafted features, and they highlight the limitations of LBP+KNN compared to modern deep learning approaches.

---

## Requirements

Main Python dependencies:

- `numpy`
- `matplotlib`
- `scikit-image`
- `scikit-learn`
- `jupyter` 

```bash
pip install numpy matplotlib scikit-image scikit-learn jupyter
```


---

## How to run

1. Place the FER‑2013 images (or equivalent dataset) inside the `dataset/train` and `dataset/test` folders, using the directory structure described above.
2. Open the notebook:

```bash
jupyter notebook knn_recognition.ipynb
```

3. Run all cells from top to bottom:
   - The notebook will load the dataset, extract LBP features, train the KNN classifier, and print the evaluation metrics.
   - A helper function at the end of the notebook allows you to visualise a test image together with its predicted emotion.

