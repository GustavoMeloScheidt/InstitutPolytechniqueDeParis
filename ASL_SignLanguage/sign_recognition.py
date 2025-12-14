import cv2
import mediapipe as mp
import socket
import time
import numpy as np

# ===== FIX MACOS =====
import os
os.environ['OPENCV_VIDEOIO_PRIORITY_MSMF'] = '0'

try:
    cv2.namedWindow("test", cv2.WINDOW_NORMAL)
    cv2.destroyWindow("test")
    print("✓ OpenCV GUI backend OK")
except:
    print("⚠ OpenCV GUI might have issues")

print("="*60)
print("ASL RECOGNITION SERVER")
print("="*60)

# ===== CONFIGURATION RÉSEAU =====
def get_local_ip():
    try:
        s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        s.connect(("8.8.8.8", 80))
        local_ip = s.getsockname()[0]
        s.close()
        return local_ip
    except:
        return "Unable to detect"

HOST = "0.0.0.0"
PORT = 6000

print(f"PC IP: {get_local_ip()}")
print(f"Port: {PORT}")
print("="*60)

# ===== MEDIAPIPE =====
print("\nInitializing MediaPipe...")
mp_hands = mp.solutions.hands
mp_drawing = mp.solutions.drawing_utils
mp_drawing_styles = mp.solutions.drawing_styles
hands = mp_hands.Hands(
    static_image_mode=False,
    max_num_hands=1,
    min_detection_confidence=0.7,
    min_tracking_confidence=0.7
)
print("✓ MediaPipe OK")

def classify_asl_letter(landmarks):
    fingers_up = []
    
    thumb_tip = landmarks[4]
    thumb_ip = landmarks[3]
    if thumb_tip.x < thumb_ip.x - 0.04:
        fingers_up.append(1)
    else:
        fingers_up.append(0)
    
    tips = [landmarks[8], landmarks[12], landmarks[16], landmarks[20]]
    pips = [landmarks[6], landmarks[10], landmarks[14], landmarks[18]]
    
    for tip, pip in zip(tips, pips):
        if tip.y < pip.y - 0.03:
            fingers_up.append(1)
        else:
            fingers_up.append(0)
    
    # Classification simple
    if fingers_up == [1, 0, 0, 0, 0]:
        return "A"
    if fingers_up == [0, 1, 1, 1, 1]:
        return "B"
    if fingers_up == [0, 0, 0, 0, 1]:
        return "I"
    if fingers_up == [1, 1, 0, 0, 0]:
        return "L"
    if fingers_up == [0, 1, 0, 0, 0]:
        return "1"
    if fingers_up == [0, 1, 1, 0, 0]:
        return "V"
    if fingers_up == [0, 1, 1, 1, 0]:
        return "W"
    if fingers_up == [1, 0, 0, 0, 1]:
        return "Y"
    if fingers_up == [1, 1, 1, 1, 1]:
        return "5"
    
    return None

def draw_ui(frame, letter, word, nao_connected, time_left=0):
    h, w, _ = frame.shape
    
    # Header
    cv2.rectangle(frame, (0, 0), (w, 80), (40, 40, 40), -1)
    cv2.putText(frame, "ASL RECOGNITION SYSTEM", (20, 50),
                cv2.FONT_HERSHEY_SIMPLEX, 1.5, (255, 255, 255), 3)
    
    # Status NAO
    color = (0, 255, 0) if nao_connected else (0, 0, 255)
    status_text = "CONNECTED" if nao_connected else "DISCONNECTED"
    cv2.circle(frame, (w - 200, 40), 10, color, -1)
    cv2.putText(frame, f"NAO {status_text}", (w - 180, 48),
                cv2.FONT_HERSHEY_SIMPLEX, 0.7, (255, 255, 255), 2)
    
    # Panneau info
    panel_x = w - 400
    overlay = frame.copy()
    cv2.rectangle(overlay, (panel_x, 90), (w - 10, h - 10), (30, 30, 30), -1)
    cv2.addWeighted(overlay, 0.8, frame, 0.2, 0, frame)
    cv2.rectangle(frame, (panel_x, 90), (w - 10, h - 10), (100, 100, 100), 2)
    
    y = 130
    
    # Lettre
    cv2.putText(frame, "DETECTED LETTER", (panel_x + 20, y),
                cv2.FONT_HERSHEY_SIMPLEX, 0.8, (150, 150, 150), 2)
    y += 50
    
    if letter:
        cv2.putText(frame, letter, (panel_x + 150, y + 60),
                    cv2.FONT_HERSHEY_SIMPLEX, 5, (0, 255, 0), 8)
        cv2.rectangle(frame, (panel_x + 20, y), (w - 30, y + 100), (0, 150, 0), 3)
    else:
        cv2.putText(frame, "---", (panel_x + 160, y + 60),
                    cv2.FONT_HERSHEY_SIMPLEX, 4, (80, 80, 80), 4)
        cv2.rectangle(frame, (panel_x + 20, y), (w - 30, y + 100), (50, 50, 50), 2)
    
    y += 130
    
    # Mot
    cv2.line(frame, (panel_x + 20, y), (w - 30, y), (80, 80, 80), 1)
    y += 40
    cv2.putText(frame, "CURRENT WORD", (panel_x + 20, y),
                cv2.FONT_HERSHEY_SIMPLEX, 0.8, (150, 150, 150), 2)
    y += 50
    
    word_color = (255, 255, 0) if word != "(empty)" else (80, 80, 80)
    cv2.putText(frame, word, (panel_x + 20, y),
                cv2.FONT_HERSHEY_SIMPLEX, 1.3, word_color, 3)
    
    y += 70
    
    # Timer
    if time_left > 0:
        cv2.line(frame, (panel_x + 20, y), (w - 30, y), (80, 80, 80), 1)
        y += 35
        cv2.putText(frame, f"Auto-send: {time_left:.1f}s", (panel_x + 20, y),
                    cv2.FONT_HERSHEY_SIMPLEX, 0.7, (100, 200, 255), 2)
        y += 30
        
        # Barre
        bar_w = 350
        progress = max(0, 1 - (time_left / 3.0))
        fill = int(bar_w * progress)
        cv2.rectangle(frame, (panel_x + 20, y), (panel_x + 20 + bar_w, y + 15), (40, 40, 40), -1)
        cv2.rectangle(frame, (panel_x + 20, y), (panel_x + 20 + fill, y + 15), (100, 200, 255), -1)
        cv2.rectangle(frame, (panel_x + 20, y), (panel_x + 20 + bar_w, y + 15), (80, 80, 80), 2)
    
    # Contrôles
    y = h - 130
    cv2.line(frame, (panel_x + 20, y), (w - 30, y), (80, 80, 80), 1)
    y += 35
    cv2.putText(frame, "CONTROLS", (panel_x + 20, y),
                cv2.FONT_HERSHEY_SIMPLEX, 0.8, (150, 150, 150), 2)
    y += 30
    
    controls = [("SPACE", "Send"), ("BACK", "Delete"), ("Q", "Quit")]
    for key, action in controls:
        cv2.putText(frame, f"{key}: {action}", (panel_x + 20, y),
                    cv2.FONT_HERSHEY_SIMPLEX, 0.6, (200, 200, 200), 1)
        y += 25
    
    return frame

# ===== CAMERA =====
print("\nOpening webcam...")
cap = cv2.VideoCapture(0)
time.sleep(1.5)

if not cap.isOpened():
    print("✗ Cannot open camera!")
    exit(1)

cap.set(cv2.CAP_PROP_FRAME_WIDTH, 1280)
cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 720)

ret, _ = cap.read()
if not ret:
    print("✗ Cannot read from camera!")
    cap.release()
    exit(1)

print(f"✓ Camera OK")

# ===== CRÉER FENÊTRE =====
print("\n🖼️  Opening window...")
cv2.namedWindow("ASL Recognition", cv2.WINDOW_NORMAL)
cv2.resizeWindow("ASL Recognition", 1280, 720)

# ===== SOCKET =====
print("\nStarting server...")
server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
server.settimeout(0.1)

try:
    server.bind((HOST, PORT))
except OSError as e:
    print(f"✗ Port error: {e}")
    cap.release()
    cv2.destroyAllWindows()
    exit(1)

server.listen(1)
print(f"✓ Listening on {get_local_ip()}:{PORT}")
print("\n" + "="*60)
print("WAITING FOR NAO...")
print("="*60)

# ===== ATTENTE NAO (sans accumulation) =====
nao_connected = False
conn = None

while not nao_connected:
    ret, frame = cap.read()
    if ret:
        frame = cv2.flip(frame, 1)
        rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        results = hands.process(rgb)
        
        letter = None
        if results.multi_hand_landmarks:
            lm = results.multi_hand_landmarks[0].landmark
            letter = classify_asl_letter(lm)
            
            mp_drawing.draw_landmarks(
                frame,
                results.multi_hand_landmarks[0],
                mp_hands.HAND_CONNECTIONS,
                mp_drawing_styles.get_default_hand_landmarks_style(),
                mp_drawing_styles.get_default_hand_connections_style()
            )
        
        # Afficher sans accumuler
        frame = draw_ui(frame, letter, "(waiting NAO)", False, 0)
        cv2.putText(frame, "Waiting for NAO connection...", (50, 400),
                    cv2.FONT_HERSHEY_SIMPLEX, 1.3, (255, 150, 0), 3)
        cv2.imshow("ASL Recognition", frame)
    
    # Check NAO
    try:
        conn, addr = server.accept()
        conn.setsockopt(socket.SOL_SOCKET, socket.SO_KEEPALIVE, 1)
        print(f"\n✓✓✓ NAO CONNECTED: {addr}")
        print("Starting word detection NOW!\n")
        nao_connected = True
        break
    except socket.timeout:
        pass
    
    key = cv2.waitKey(1) & 0xFF
    if key == ord('q') or key == 27:
        print("\nQuitting...")
        cap.release()
        cv2.destroyAllWindows()
        hands.close()
        server.close()
        exit(0)

# ===== BOUCLE PRINCIPALE (APRÈS connexion NAO) =====
print("="*60)
print("SYSTEM ACTIVE - START SHOWING SIGNS NOW!")
print("="*60 + "\n")

word_buffer = []
last_letter = None
last_time = time.time()
letter_cooldown = 1.5
word_send_delay = 3.0
last_word_time = time.time()

try:
    while True:
        ret, frame = cap.read()
        if not ret:
            time.sleep(0.05)
            continue
        
        frame = cv2.flip(frame, 1)
        rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        results = hands.process(rgb)
        
        letter = None
        if results.multi_hand_landmarks:
            lm = results.multi_hand_landmarks[0].landmark
            letter = classify_asl_letter(lm)
            
            mp_drawing.draw_landmarks(
                frame,
                results.multi_hand_landmarks[0],
                mp_hands.HAND_CONNECTIONS,
                mp_drawing_styles.get_default_hand_landmarks_style(),
                mp_drawing_styles.get_default_hand_connections_style()
            )
        
        # ✅ AJOUTER AU BUFFER (maintenant actif!)
        if letter:
            now = time.time()
            if letter != last_letter and (now - last_time > letter_cooldown):
                word_buffer.append(letter)
                last_letter = letter
                last_time = now
                last_word_time = now
                print(f"[+] Added: {letter} → Word: {''.join(word_buffer)}")
        
        # Timer
        time_left = word_send_delay - (time.time() - last_word_time) if word_buffer else 0
        
        word = ''.join(word_buffer) if word_buffer else "(empty)"
        frame = draw_ui(frame, letter, word, nao_connected, time_left)
        
        # ✅ ENVOI AUTO
        if len(word_buffer) > 0 and time_left <= 0:
            word_to_send = ''.join(word_buffer)
            print(f"\n{'='*50}")
            print(f"[→→→] SENDING TO NAO: '{word_to_send}'")
            print(f"{'='*50}")
            
            try:
                data = word_to_send.encode("utf-8") + b"\n"
                conn.sendall(data)
                print(f"✓✓✓ SENT: '{word_to_send}' ({len(data)} bytes)")
                print(f"{'='*50}\n")
                
                word_buffer = []
                last_letter = None
            except Exception as e:
                print(f"✗✗✗ ERROR: {e}")
                import traceback
                traceback.print_exc()
                break
        
        cv2.imshow("ASL Recognition", frame)
        
        key = cv2.waitKey(1) & 0xFF
        if key == ord('q') or key == 27:
            break
        elif key == ord(' ') and word_buffer:
            word_to_send = ''.join(word_buffer)
            print(f"\n[MANUAL] Sending: '{word_to_send}'")
            try:
                conn.sendall((word_to_send + "\n").encode("utf-8"))
                print(f"✓ Sent!\n")
                word_buffer = []
                last_letter = None
                last_word_time = time.time()
            except Exception as e:
                print(f"✗ Error: {e}")
                break
        elif key == 8 and word_buffer:
            removed = word_buffer.pop()
            print(f"[-] Removed: {removed}")
            last_word_time = time.time()

except KeyboardInterrupt:
    print("\n[!] Interrupted")
except Exception as e:
    print(f"\n✗ Error: {e}")
    import traceback
    traceback.print_exc()
finally:
    print("\nShutting down...")
    cap.release()
    cv2.destroyAllWindows()
    hands.close()
    try:
        if conn:
            conn.close()
        server.close()
    except:
        pass
    print("✓ Done")