# DirectX_Base
This is a basis from which to start implementing a 2D DirectX11 scene in a Windows Form.

To implement sprites, modify Resources/Sprites.png to contain whatever sprites you will use, building an array going left to right. Then, modify TextureIndeces.txt, appending the parameters defining each sprite in the same way the Alphabet texture is already defined.
The format is like this:

[Give the sprite a name][tab][Pixel the sprite starts at][tab][Pixels across the sprite is][tab]1 // 1 at the end means it will use Sprites.png rather than Alphabet.png
