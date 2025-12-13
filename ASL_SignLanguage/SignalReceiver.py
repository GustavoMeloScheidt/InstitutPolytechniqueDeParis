class MyClass(GeneratedClass):
    def __init__(self):
        GeneratedClass.__init__(self)
        from naoqi import ALProxy
        self.tts = ALProxy("ALTextToSpeech")
        self.motion = ALProxy("ALMotion")
        self.posture = ALProxy("ALRobotPosture")

    def onInput_onStart(self):
        import socket
        import time
        import random

        # Configuration TTS
        self.tts.setVolume(1.0)
        self.tts.say("Starting A S L recognition")

        #HOST = "127.0.0.1"
        HOST = "192.168.2.139"
        PORT = 6000

        s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        s.settimeout(10.0)

        # Connexion
        connected = False
        for i in range(30):
            try:
                s.connect((HOST, PORT))
                connected = True
                break
            except:
                time.sleep(0.2)

        if not connected:
            self.tts.say("Connection failed")
            self.onStopped()
            return

        self.tts.say("Connected successfully")

        # Activer les moteurs et position initiale
        try:
            self.motion.wakeUp()
            time.sleep(1)

            # Position de départ naturelle
            self.reset_to_neutral_position()

        except Exception as e:
            print("Motion setup error: " + str(e))

        buffer = ""
        word_count = 0

        # BOUCLE PRINCIPALE
        while True:
            try:
                data = s.recv(1024)

                if not data:
                    break

                decoded = data.decode("utf-8")
                buffer = buffer + decoded

                # Traiter les lignes complètes
                while "\n" in buffer:
                    parts = buffer.split("\n", 1)
                    word = parts[0].strip()
                    buffer = parts[1]

                    if word and len(word) > 0:
                        word_count = word_count + 1

                        # Choisir un geste aléatoire pour varier
                        gesture_choice = random.randint(1, 5)

                        # PARLER
                        speech = "You spelled " + str(word)
                        self.tts.say(speech)

                        time.sleep(0.3)

                        # GESTE selon le choix aléatoire
                        try:
                            if gesture_choice == 1:
                                self.celebrate_both_arms()
                            elif gesture_choice == 2:
                                self.thumbs_up()
                            elif gesture_choice == 3:
                                self.clap_hands()
                            elif gesture_choice == 4:
                                self.wave_hello()
                            else:
                                self.point_forward()
                        except Exception as e:
                            print("Gesture error: " + str(e))

                        # Retour position neutre
                        time.sleep(0.5)
                        self.reset_to_neutral_position()
                        time.sleep(0.3)

            except socket.timeout:
                continue
            except socket.error:
                break
            except Exception as e:
                print("Loop error: " + str(e))
                continue

        # Nettoyage
        try:
            s.close()
        except:
            pass

        try:
            self.motion.rest()
        except:
            pass

        self.tts.say("Recognition stopped")
        self.onStopped()

    def onInput_onStop(self):
        try:
            self.motion.rest()
        except:
            pass
        self.onStopped()


    # =============================================
    #          POSITIONS ET GESTES
    # =============================================

    def reset_to_neutral_position(self):
        """Position neutre et naturelle"""
        import time
        try:
            # Tête droite
            self.motion.setAngles(["HeadYaw", "HeadPitch"], [0.0, 0.0], 0.2)

            # Bras le long du corps, légèrement relâchés
            names_r = ["RShoulderPitch", "RShoulderRoll", "RElbowYaw", "RElbowRoll"]
            angles_r = [1.4, -0.15, 1.2, 0.5]

            names_l = ["LShoulderPitch", "LShoulderRoll", "LElbowYaw", "LElbowRoll"]
            angles_l = [1.4, 0.15, -1.2, -0.5]

            times = [0.8, 0.8, 0.8, 0.8]

            self.motion.post.angleInterpolation(names_r, angles_r, times, True)
            self.motion.post.angleInterpolation(names_l, angles_l, times, True)

            time.sleep(0.8)
        except:
            pass


    # =============================================
    #          GESTES VARIÉS
    # =============================================

    def celebrate_both_arms(self):
        """Lever les deux bras en célébration"""
        import time
        try:
            # Lever les deux bras simultanément
            names_r = ["RShoulderPitch", "RShoulderRoll", "RElbowYaw", "RElbowRoll"]
            angles_r = [-0.8, -0.3, 1.5, 1.4]

            names_l = ["LShoulderPitch", "LShoulderRoll", "LElbowYaw", "LElbowRoll"]
            angles_l = [-0.8, 0.3, -1.5, -1.4]

            times = [0.6, 0.6, 0.6, 0.6]

            # Mouvement synchronisé
            self.motion.post.angleInterpolation(names_r, angles_r, times, True)
            self.motion.angleInterpolation(names_l, angles_l, times, True)

            time.sleep(0.8)
        except:
            pass

    def thumbs_up(self):
        """Pouce levé avec le bras droit"""
        import time
        try:
            # Lever le bras droit sur le côté
            names = ["RShoulderPitch", "RShoulderRoll", "RElbowYaw", "RElbowRoll"]

            # Position 1: lever le bras
            angles1 = [0.0, -0.8, 1.5, 1.0]
            times1 = [0.5, 0.5, 0.5, 0.5]
            self.motion.angleInterpolation(names, angles1, times1, True)

            time.sleep(0.5)

            # Position 2: petit mouvement pour accentuer
            angles2 = [0.0, -0.9, 1.5, 1.2]
            times2 = [0.3, 0.3, 0.3, 0.3]
            self.motion.angleInterpolation(names, angles2, times2, True)

            time.sleep(0.3)
        except:
            pass

    def clap_hands(self):
        """Applaudir"""
        import time
        try:
            # Position de départ : bras devant
            names_r = ["RShoulderPitch", "RShoulderRoll", "RElbowYaw", "RElbowRoll"]
            names_l = ["LShoulderPitch", "LShoulderRoll", "LElbowYaw", "LElbowRoll"]

            # Bras devant, écartés
            angles_r_open = [0.5, -0.5, 1.0, 1.0]
            angles_l_open = [0.5, 0.5, -1.0, -1.0]
            times = [0.4, 0.4, 0.4, 0.4]

            # 3 applaudissements
            for i in range(3):
                # Écarter
                self.motion.post.angleInterpolation(names_r, angles_r_open, times, True)
                self.motion.angleInterpolation(names_l, angles_l_open, times, True)
                time.sleep(0.4)

                # Rapprocher (applaudir)
                angles_r_close = [0.5, -0.2, 0.5, 0.8]
                angles_l_close = [0.5, 0.2, -0.5, -0.8]
                times_fast = [0.2, 0.2, 0.2, 0.2]

                self.motion.post.angleInterpolation(names_r, angles_r_close, times_fast, True)
                self.motion.angleInterpolation(names_l, angles_l_close, times_fast, True)
                time.sleep(0.2)
        except:
            pass

    def wave_hello(self):
        """Faire un signe de la main (wave)"""
        import time
        try:
            names = ["RShoulderPitch", "RShoulderRoll", "RElbowYaw", "RElbowRoll", "RWristYaw"]

            # Lever le bras
            angles_up = [-0.5, -0.5, 1.2, 0.3, 0.0]
            times_up = [0.6, 0.6, 0.6, 0.6, 0.6]
            self.motion.angleInterpolation(names, angles_up, times_up, True)

            # Mouvement de wave (rotation du poignet et coude)
            for i in range(4):
                # Gauche
                self.motion.setAngles(["RElbowRoll", "RWristYaw"], [0.1, -0.5], 0.5)
                time.sleep(0.25)
                # Droite
                self.motion.setAngles(["RElbowRoll", "RWristYaw"], [0.5, 0.5], 0.5)
                time.sleep(0.25)
        except:
            pass

    def point_forward(self):
        """Pointer du doigt vers l'avant"""
        import time
        try:
            names = ["RShoulderPitch", "RShoulderRoll", "RElbowYaw", "RElbowRoll"]

            # Tendre le bras vers l'avant
            angles = [0.0, -0.1, 1.5, 0.1]
            times = [0.6, 0.6, 0.6, 0.6]

            self.motion.angleInterpolation(names, angles, times, True)

            time.sleep(0.5)

            # Petit mouvement d'emphase
            angles2 = [-0.1, -0.1, 1.5, 0.1]
            times2 = [0.3, 0.3, 0.3, 0.3]
            self.motion.angleInterpolation(names, angles2, times2, True)

            time.sleep(0.3)
        except:
            pass

    def nod_yes(self):
        """Hocher la tête pour dire oui"""
        import time
        try:
            for i in range(3):
                # Bas
                self.motion.setAngles("HeadPitch", 0.3, 0.5)
                time.sleep(0.3)
                # Haut
                self.motion.setAngles("HeadPitch", -0.2, 0.5)
                time.sleep(0.3)

            # Retour position neutre
            self.motion.setAngles("HeadPitch", 0.0, 0.3)
        except:
            pass

    def shake_head_no(self):
        """Secouer la tête pour dire non"""
        import time
        try:
            for i in range(3):
                # Gauche
                self.motion.setAngles("HeadYaw", 0.5, 0.5)
                time.sleep(0.3)
                # Droite
                self.motion.setAngles("HeadYaw", -0.5, 0.5)
                time.sleep(0.3)

            # Retour position neutre
            self.motion.setAngles("HeadYaw", 0.0, 0.3)
        except:
            pass

    def thinking_pose(self):
        """Position de réflexion (main sur le menton)"""
        import time
        try:
            # Lever le bras droit vers le visage
            names = ["RShoulderPitch", "RShoulderRoll", "RElbowYaw", "RElbowRoll"]
            angles = [-0.3, -0.1, 1.0, 1.2]
            times = [0.8, 0.8, 0.8, 0.8]

            self.motion.angleInterpolation(names, angles, times, True)

            # Incliner légèrement la tête
            self.motion.setAngles("HeadPitch", 0.2, 0.3)

            time.sleep(1.0)
        except:
            pass