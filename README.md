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
- The following addresses are used for OSC messaging (port=9000).

| Address     | Trigger Type |
| ----------- | ------------ |
| `/trig/1-1` | Kick         |
| `/trig/1-3` | Kick         |
| `/trig/1-2` | Snare        |

License
-------

[MIT](LICENSE.txt)
