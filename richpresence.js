(() => {
    const version = 1;  //Current Version
  
    //Bootstrap
    if (!window.DiscordRichPresence) {
      window.DiscordRichPresence = {loaded: 0};
      window.DiscordRichPresence.setup = () => console.error('Discord Rich Presence | Failed to setup Discord Rich Presence');
      $(() => window.DiscordRichPresence.setup());
    }
  
    if (window.DiscordRichPresence.loaded >= version) {
      return;
    }
    window.DiscordRichPresence.loaded = version;

    function getCurrentSceneName() {
      var scenes = game.scenes.entities;
      var firstActiveScene = scenes.find(function(element) { return element.active; });

      if (!firstActiveScene) return "Unknown";

      return firstActiveScene.name;
    }

    function getCurrentActorName() {
      return game.actors.entities[1].token.name;
    }

    function getCurrentActorHealth() {
      return game.actors.entities[1].data.health.value;
    }

    function getCurrentActorMaxHealth() {
      return game.actors.entities[1].data.health.max;
    }
  
    window.DiscordRichPresence.setup = () => {
      console.log(`Discord Rich Presence | Initializing v${version}`);
  
      Hooks.on('ready', () => {
        console.log("Forge initited!");
        console.log("Player is currently on scene " + getCurrentSceneName());
        console.log("Player is currently playing Actor " + getCurrentActorName() + " which has health " + getCurrentActorHealth() + " / " + getCurrentActorMaxHealth());
      });
    };
  })();
  