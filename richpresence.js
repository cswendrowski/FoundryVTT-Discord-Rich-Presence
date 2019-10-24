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
      var currentUser = game.user.character;

      if (!currentUser) return "";

      return currentUser.name;
    }

    function getSystemName() {
      return game.system.data.title;
    }

    function getCurrentPlayers() {
      return game.users.entities.filter(element => element.active ).length;
    }

    function getMaxPlayers() {
      return game.users.entities.length;
    }

    function getCurrentPlayerIsGm() {
      return game.user.isGM;
    }

    function getCurrentGameRemoteUrl() {
      return game.data.ips.remote;
    }

    function getUniqueWorldId() {
      return game.data.world.id;
    }

    class PlayerStatus {
      constructor()
      {
        this.SceneName = getCurrentSceneName();
        this.ActorName = getCurrentActorName();
        this.CurrentPlayerCount = getCurrentPlayers();
        this.MaxPlayerCount = getMaxPlayers();
        this.IsGm = getCurrentPlayerIsGm();
        this.FoundryUrl = getCurrentGameRemoteUrl();
        this.WorldUniqueId = getUniqueWorldId();
        this.SystemName = getSystemName();
      }
    }

    // Gross
    function sleep(milliseconds) {
      var start = new Date().getTime();
      for (var i = 0; i < 1e7; i++) {
        if ((new Date().getTime() - start) > milliseconds){
          break;
        }
      }
    }

    function sendPlayerStatusUpdate() {
      var url = 'http://localhost:2324/api/PlayerStatus';
      var json = JSON.stringify(new PlayerStatus());
      var otherParams = {
        headers: {
          'Content-Type': 'application/json'
        },
        body: json,
        method: 'POST'
      };

      fetch(url, otherParams)
        .then(res => { console.log(res) })
        .catch(error => console.log(error));
    }

    window.onbeforeunload = function()
    { 
      console.log("Discord Rich Presence | Unloading");
      var url = 'http://localhost:2324/api/PlayerStatus/leave';
      var otherParams = {
        method: 'POST'
      };

      fetch(url, otherParams)
        .then(res => { console.log(res) })
        .catch(error => console.log(error));

      sleep(100);
      
      console.log("Discord Rich Presence | Done!");
    }
  
    window.DiscordRichPresence.setup = () => {
      console.log(`Discord Rich Presence | Initializing v${version}`);

      Hooks.on('ready', () => {
        sendPlayerStatusUpdate();

        setInterval(function() { sendPlayerStatusUpdate(); }, 15 * 1000);
      });
    };
  })();
  