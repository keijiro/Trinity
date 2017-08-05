Trinity
=======

**Trinity** is a Unity project that was used for live visuals at [Channel 17].

![picture](http://i.imgur.com/BZEdISjm.jpg)
![gif](http://i.imgur.com/eF20IDe.gif)

[Channel 17]: https://www.super-deluxe.com/room/4329/

System Requirements
-------------------

- Unity 2017.1.0 or later
- Windows or macOS system
- Four XGA (1024x768) displays

Trinity is designed for triple-projector setups. It uses the first (leftmost)
screen for control/monitoring, and the latter three screens for projection. So
it needs four displays in total.

Note
----

- The default recording device in the system configuration is used for audio
  analysis.
- The following addresses are used for OSC messaging.

| Address     | Trigger Type |
| ----------- | ------------ |
| `/trig/1-1` | Kick         |
| `/trig/1-3` | Kick         |
| `/trig/1-2` | Snare        |

- The port number for OSC messaging is fixed to 9000.
- You can also use Z (kick) and X (snare) keys for triggering. This is useful
  when OSC is not an option or network connection is lost while performance.

License
-------

[MIT](LICENSE.txt)
