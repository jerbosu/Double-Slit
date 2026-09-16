## About
A 2D top down action game built in Unity. My first game coding project. Inspired by many other games, as well as the experiment of the same name. 

## Objectives
### Week 1 (Aug 2-8)
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

### Week 2 (Aug 9-15)
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
- [x] Allow max velocity to be exceeded through a combination of player movement and recoil, but not player movement alone
    - Solution: instead of clamping at max velocity, lerp downwards instead with a lower speed cap. The lerp and addForce find a equilibrium slightly above the set speed cap, but bursts are still allowed to briefly push past it. 
- [x] Clean up inside of main functions (mainly Update and FixedUpdate) to not have scattered code everywhere

</details>

<details>
<summary>Gameplay preparation</summary>

- [x] Bounding arena box
- [x] Change cooldowns and particle systems to use unscaledTime in preparation for hitstop implementation
    - This prevents the heavy attack indicator from desyncing, and also rewards landing a heavy attack or gives leniency when taking a strong attack 

</details>

### Week 3 (Aug 16-22)
<details>
    <summary>Misc</summary>

- [x] Reorganize scripts folder (will probably do the same with other asset folders in the future)
- [x] Separate the squash/stretch code from the player so that it can be used for separate gameobjects
    - Created a new squash/stretch script, which can be attached to the visual of anything that needs it
- [x] Have heavy attack inherit the velocity of the player
- [ ] Possibly change heavy attack to a single object that fades away rather than a sprite animation?

</details>

<details> 
    <summary>First enemy</summary>

- [x] Enemy visuals
    - [x] Sprite
    - [x] Squash/stretch (very easy now that squash/stretch is its own script)
    - [ ] Attack indicator
        - [x] Attack telegraph script (rectangle)
            - This will scale Unity's basic square to the size of the attack hitbox, and fill it up to show how long until the attack happens. 
        - [ ] Implement prefab on enemy
- [x] Enemy collider
- [ ] Enemy behaviour (this was so much more difficult than I thought it would be)
    - [x] Move towards/away from player if too far/close
    - [x] Occasionally idle
    - [x] Somewhat circle player at a set distance for a random amount of time
    - [x] Attack player when time is up
    - [x] Temporary retreat after attacking or after taking significant damage

- [x] Rewrite enemy control script to use IEnum and case switching instead of if else spaghetti
    - It's still spaghetti but it's more readable
</details>

### Week 4 (Aug 23 - 29)
<details>
    <summary>Misc</summary>

- [x] Refactor state machine code to make it more readable again. 
    - Turns out AI is really good at making your code look nice. It's not the best at making functional code though. Or at least the free model isn't. 
- [x] Disable player hitbox while dashing (i.e. can move through enemies and attacks, but not through walls)
- [x] Add player healthbar display
- [ ] Player heavy attack hitbox animation over time
    - As the heavy attack is a shockwave that moves forward, the hitbox must be animated accordingly
- [x] Created a general Hurtbox and Hitbox script to use in all attacks

</details>

<details>
    <summary>Finishing up enemy 1 visuals and behaviour</summary>

- [x] Enemy behaviour (finally done to a sastifactory level)
    - [x] Move towards/away from player if too far/close
    - [x] Occasionally idle
        - Idle until approached by player, then continually seeks out player
    - [x] Somewhat circle player at a set distance for a random amount of time
        - Proper circular motion achieved
    - [x] Attack player when time is up
        - [x] Greater damping right after the attack so the enemy doesn't fly off into space
    - [x] Temporary retreat after attacking or after taking significant damage

- [x] Enemy visuals
    - [x] Attack indicator
        - [x] Attack telegraph script (rectangle)
            - This will scale a square to show the AoE of the attack. The attack timing will be shown with a flash. 
        - [x] Implement prefab on enemy
        - [x] Fix the indicator (it looks like a stream of rectangles rather than a tracking AoE)
            - A new coroutine was called every FixedUpdate. Fixed by checking whether the coroutine was null before calling. 

- [x] Enemy hurtbox
    - [x] Physics interaction after getting hit by the player (i.e. slight knockback for light attack)
        - Requires significant tweaking to "feel" correct
        - Current knockback is based on player and enemy position, not attack direction
    - [ ] Particle effect on death (direction, velocity...)


</details>

### Week 5 (Aug 30 - Sept 5)
Note that the current version has a bug with the way enemy1 attempts to predict the player movement when launching an attack. 
<details>
    <summary>Misc</summary>

- [x] On hit effects
    - [x] Particles on hit (speed scales with damage)
    - [x] Flash on hit
- [x] Animate heavy attack hitbox
    - [ ] Either increase accuracy of the animation or change heavy attack to match the animation
- [x] Hurtbox/collider correction
    - Capsule colliders can only extend along one axis. Matched that axis with the squash/stretch axis for the two gameobjects currently using them. 
- [x] Hitbox correction
    - Sometimes the knockback from an attack could cause an enemy to enter, exit, then enter the attack hitbox again, dealing multiple instances of knockback and damage
    - Fixed by storing and checking if an entity has already been hit by an attack instance
- [ ] Have UI show on top of gameobjects
- [x] Change enemy1 attack range to be distance based rather than time based. Very silly error.
    - Telegraph prefab and hitbox are now also distance rather than time based.

</details>

<details>
    <summary>Enemy 1 interactions</summary>

- [x] Enemy hurtbox
    - [x] Physics interaction after getting hit by the player (i.e. slight knockback for light attack)
        - Requires significant tweaking to "feel" correct
        - Current knockback is based on player and enemy position, not attack direction. Maybe change later
    - [x] Particle effect on death (direction, velocity...)
        - Currently uses onHit particle effect
    - [x] Fixed bug where attack telegraph would remain after death
        - By setting the enemy as the telegraph's parent, it would be destroyed with the enemy on death

- [x] Attack hitbox
    - [ ] Physics interaction on hitting the player
    - [ ] Player health, respawn mechanics
    - [x] Player recovers hp upon hitting the enemy

- [x] Parrying!!!!
    - [x] General parry script (so enemies can parry as well)
    - [x] Enemy parry window
        - [x] Fixed a bug where parrying wouldn't deal damage or knockback
    - [x] Hitstop on parry
        - [x] Fix player heavy attack indicator not working despite using unscaled time
            - Solution: the particle system used as indicator was set to scaled time.
    - [x] Screen flash on parry
        - The ScreenFlash class can also be used to indicate other effects like damage taken, etc 

- [x] Minor enemy1 behaviour changes
    - Much more aggressive, circles and idles for less time
    - Increased attack range
    - Slower "bounce back" after a parry
    - [x] Attempted to add player movement prediction
    
- [x] Enemy spawning script
    - Currently spawns at a set interval for easier testing
    - [x] Make enemy1 into a prefab

- [ ] Basic pathfinding?

</details>

### Week 6 (Sept 6 - 12)

<details>
    <summary>Misc</summary>

- [x] Fixed enemy1 telegraph behaviour
- [x] Telegraph still sometimes finishes playing even if enemy dies, fix
- [x] Create a new branch to experiment with branches and merging. 

- [x] Add effects for when enemy hits the player
    - [x] Screenflash red
        - Added parameter to screenflash on whether to fade flash away or immediately remove it
    - [x] More knockback
        - [x] Fix knockback not correctly applying to player
- [ ] Add enemy1 death effect

- [x] Screenshake effect
    - Triggered on parry, on enemydeath. Maybe add on damage taken by player
    - Maybe this is overusing it?
- [x] Edit to main branch, separate from BaseEnemy branch. For experimenting with merge conflict resolution.  

</details>

<details>
    <summary>Major Code Refactor</summary>

- [ ] Clean up enemy1 code
    - [ ] Idle state seems slightly redundant/not serving its intended purpose
        - Wanted it as a "watching the player" state, cancelled upon player getting to close
- [x] Extract from enemy1 controller code a "base enemy" class for reusability
    - [x] Enemy Stats (health, movespeed, etc) are customizable per enemy
    - [x] Added base behaviour to BaseEnemy
        - [x] Moved Idle, TooFar, TooClose to BaseEnemy
        - [x] Add Circling (melee enemies only) 
        - [ ] Add Aiming (ranged enemies only)
            - Maybe make Aiming part of attack foreswing?
        - [x] Move a basic state machine to BaseEnemy (includes Idle, TooFar, TooClose)?
            - TEST IF WORKING I BET IT ISNT LOL
            - It sort of works...
    - [x] Switch enemy1 over to BaseEnemy
        - [x] Test if it works lol (wtf it does wow first try lol)
    

</details>

<details>
    <summary>Enemy 2</summary>

- Will be a ranged enemy
- [ ] Enemy2 visuals
    - [x] Sprite
    - [ ] Projectile

</details>