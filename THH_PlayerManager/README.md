**PlayerManager v2.2**

Purpose: Give every player a personal object they own and send custom network events to specific players.<br/>
Complexity Level: High

*SETUP:* Drag and drop the '[THH_PlayerManager]' prefab from the Prefabs folder into your scene. Add a PlayerHandler prefab as a child of the manager.
Unpack both prefabs and add your system to the handler. You can either modify the code of the handler directly, or parent your objects under the handler. Now duplicate the handler as many times as you need it (Maximum Players * 2 + 1 to be safe).

**MAKE SURE TO RECOMPILE ALL UDON SHARP SCRIPTS AFTER IMPORTING**
