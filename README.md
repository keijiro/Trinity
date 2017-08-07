Trinity
=======

**Trinity** is a Unity project that was used for live visuals in
[DUB-Russell]'s live performance at [Channel 17].

![gif](http://i.imgur.com/5Mx6gPX.gif)
![gif](http://i.imgur.com/eF20IDe.gif)

More photos and videos are available on [Tumblr].

[DUB-Russell]: http://dubrussell.com
[Channel 17]: https://www.super-deluxe.com/room/4329/
[Tumblr]: http://radiumsoftware.tumblr.com/tagged/channel17

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

Acknowledgements
----------------

[The dollemonx 3D model] was scanned by [MobBod] and licensed by [CODAME] under
a [Creative Commons Attribution] license. See the [CODAME Scans] repository for
further details.

[The dollemonx 3D model]: https://sketchfab.com/models/0c7ce1376ade4b23a986916befa83e31
[MobBod]: http://codame.com/projects/modbod
[CODAME]: http://codame.com/
[Creative Commons Attribution]: https://creativecommons.org/licenses/by/4.0/
[CODAME Scans]: https://github.com/keijiro/CodameScans

License
-------

[MIT](LICENSE.txt)
