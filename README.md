## About
A 2D top down action game built in Unity. My first game coding project. Inspired by many other games, as well as the experiment of the same name. 

## Objectives
### Week 1
<details>
    <summary>Base player stuff</summary>

- [x] Player sprite
    - Rigidbody2D as the parent, visual as the child
- [x] Player movement
    - [x] Fix player movement physics
        - Now uses lerp to decelerate. Previously relied on incrementally subtracting from velocity, but this oscillated when near 0. 
- [x] Movement effects
- [x] Procedural squash/stretch
    - Rotate the child visual towards the velocity vector, and scale accordingly. Max squash/stretch is based on max achievable velocity (i.e. during the dash), but this may be changed later. 
    
</details>
<details>
    <summary>Light attack</summary>

- [x] Sprite
- [x] Implementation
- [x] Hitbox (capsule collider)
- [x] Input buffer
    
</details>
<details>
    <summary>Heavy Attack</summary>

- [x] Sprite
- [x] Implementation
- [ ] Hitbox (maybe a box collider)
- [x] Cooldown + input buffer
- [x] Cooldown indicators
    
</details>

### Week 2
<details>
    <summary>Dash</summary>

- [x] Movement component
    - [x] Fix physics again
        - Original system only considered 8-direction movement, so non 45-degree angle movement (i.e. recoil from strong attack) were decelerated incorrectly (i.e. equal deceleration on x and y axis). Now uses vector based movement (deceleration along the velocity vector).
- [x] Particle effect
- [x] Cooldown + input buffer
    
</details>

<details>
    <summary>First Bug Fixes/QoL stuff</summary>

- [x] Camera follows player
- [x] Fix physics again again (no more directly setting velocity)
    - Using addForce now instead
- [x] Fix strange interaction between squash/stretching and dashing into a wall
    - Answer: running into a wall applies torque which rotates the player. By default there is no damping, so this angular momentum stays constant. Fixed by locking the Rigidbody 2D's Z rotation.
- [x] Fix jerky movement at max speed
    - Answer: the original max speed condition would allow addForce until movespeed hit the max speed. However, this accelerated the speed past the max, where it would then damp back below max speed, then get another push from addForce... etc. Fixed by doing the speed check AFTER addForce instead of before, and by clamping if it failed.
- [x] Redo the heavy attack indicator to exponentially fade in instead of just appear
    - Right now, the indicator (a single particle) is timed to finish fading in just as heavy attack becomes available. In the future, care should be taken to not desync this timing (i.e. hitstops).
- [ ] Allow max velocity to be exceeded through a combination of player movement and recoil, but not player movement alone
- [x] Clean up inside of main functions (mainly Update and FixedUpdate) to not have scattered code everywhere

</details>

<details> 
    <summary>First enemy</summary>

- [x] Bounding arena box
- [ ] Enemy sprite
- [ ] Enemy hitbox
- [ ] Hitbox interaction
- [ ] HITSTOPS: Time.timeScale = 0f for full stop. Note that Time.unscaledDeltaTime is unaffected
    - [x] Change cooldowns and particle systems to use unscaledTime. 
        - This prevents the heavy attack indicator from desyncing, and also rewards landing a strong attack or gives leniency when taking a strong attack 
- [ ] Basic pathfinding AI?
- [ ] Basic moveset?

</details>