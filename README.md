<a id="readme-top"></a>

[![Contributors][contributors-shield]][contributors-url]
[![Unlicense License][license-shield]][license-url]
[![Last Commit][last-commit-shield]][last-commit-url]

<div align="center">

  <h1 align="center">Forgotten Trial</h1>

  <p align="center">
    <br />
    <a href="https://github.com/FilipT03/rl-agent-forgotten-trial/issues/new?labels=bug">Report Bug</a>
    <a href="https://youtu.be/dhAAsh6xHao">Demo</a>
  </p>
</div>

<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#installation-steps">Installation Steps</a></li>
      </ul>
    </li>
    <li><a href="#credits">Theoretical Background</a></li>
  </ol>
</details>


# About The Project

Forgotten Trial is a reinforcement learning (RL) agent simulation designed to explore agent-based learning in dynamic environments. The project models an environment where an RL agent learns through trial and error, aiming to maximize rewards over time. This agent may encounter a variety of challenges, which will test its decision-making skills.

Main features:

- RL agent trained using state-of-the-art techniques.
- Customizable environments that can simulate different scenarios for testing agent performance.
- Reward-based learning where agents learn from rewards to optimize future decisions.
- Logging and tracking of agent behavior and performance during training.

# Getting Started

Follow these steps to set up and run the project locally.

## Installation Steps

1. Clone the repository
```
git clone https://github.com/FilipT03/rl-agent-forgotten-trial.git
cd rl-agent-forgotten-trial
```

3. Set up a virtual environment (optional but recommended):
```
python -m venv venv
source venv/bin/activate  # On Windows, use `venv\Scripts\activate`
```


3. Install mlAgents

```
pip install mlagents
```

4. Open Unity and load the Forgotten Trial Unity environment:

- In the project directory, navigate to the Unity environment folder (typically named Unity or similar) and open the .unity scene.
- Set up your environment as needed.

5. Customize the environment (optional)

You can modify the environment configuration in the config.py file to experiment with different settings and parameters for agent training.

6. Run

```
mlagents-learn [./config/movement.yaml] [--resume] # Or other config file
```

# Credits

The following 3D models were used in this project:

- **Medieval Archery Target** by CaptainHC  
  Available on [Sketchfab](https://skfb.ly/6WR8z)  
  Licensed under [Creative Commons Attribution 4.0](http://creativecommons.org/licenses/by/4.0/)

- **Recurve Bow**  
  Available on [Sketchfab](https://sketchfab.com/3d-models/recurve-bow-9cf9c65300244debbd1d04ffb87576d9)  
  (No specific license mentioned; check the Sketchfab page for details)

- **Log Wall (Game-Ready Asset)**  
  Available on [Sketchfab](https://sketchfab.com/3d-models/log-wall-game-ready-asset-788e9d67d80a4f66bb007605163abf60)  
  (No specific license mentioned; check the Sketchfab page for details)

- **Woodpile (Game-Ready Asset)**  
  Available on [Sketchfab](https://sketchfab.com/3d-models/woodpile-game-ready-asset-fcd5d5993be64ca38ef8ae2926574874)  
  (No specific license mentioned; check the Sketchfab page for details)

---


<p align="right">(<a href="#readme-top">back to top</a>)</p>


[contributors-shield]: https://img.shields.io/github/contributors/FilipT03/rl-agent-forgotten-trial.svg?style=for-the-badge
[contributors-url]: https://github.com/FilipT03/rl-agent-forgotten-trial/graphs/contributors
[license-shield]: https://img.shields.io/github/license/FilipT03/rl-agent-forgotten-trial.svg?style=for-the-badge
[license-url]: https://github.com/FilipT03/rl-agent-forgotten-trial/blob/main/LICENSE
[last-commit-shield]: https://img.shields.io/github/last-commit/FilipT03/rl-agent-forgotten-trial?style=for-the-badge
[last-commit-url]: https://github.com/FilipT03/rl-agent-forgotten-trial/commits/main
