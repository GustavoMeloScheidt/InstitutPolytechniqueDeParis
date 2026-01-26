# Whack-A-Mole VR (Meta Quest)

A **Whack-A-Mole VR game** developed in **Unity** for **Meta Quest**, where the player grabs a hammer and physically hits moles as they pop out of holes.

The project focuses on **basic VR interaction**, collision handling, and gameplay logic.

---

## Run 

### Requirements

* Unity (tested with Unity 6)
* Meta Quest headset (Quest 2 / Quest 3 / Quest Pro)
* Meta Quest Link or Air Link
* Meta XR / Oculus Integration installed in the project

### Steps

1. Clone the repository:

   ```bash
   git clone...
   ```

2. Open the project in **Unity Hub**.

3. Make sure **Android** is selected as the Build Platform.

4. Connect your Meta Quest headset using **Link** or **Air Link**.

5. Open the main scene of the project.

6. Press **Play** in the Unity Editor **(for basic testing)** or **Build & Run** to deploy to the headset.

---

## Controls

* Grab the hammer using the controller
* Swing the hammer to hit the moles
* Avoid hitting bombs

---

## Notes

* The hammer uses trigger-based collisions for stability in VR
* Gameplay logic is handled by a central `GameManager`
* The project is intended for educational and experimental use

