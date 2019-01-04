# CTFAI

### A simple capture the flag game with movement controlled mainly by wasd, which gives the following:

- Move forward	(w)
- Rotate left	(a)
- Rotate right	(d)
- Shoot		(s)

The AI in the game is controlled by a neural network taking inputs given from the environment.
## Development
The program is developed in Unity using c#.
The AI can only see enemies that are within field of view, which is given by a circle around it cut with an angle to "humanize" its behaviour.
The agent collects input like:

- Am I holding the flag?
- Am I in my base?
- Can I see an enemy?
- ...