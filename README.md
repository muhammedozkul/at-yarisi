**English** · [Türkçe](README.tr.md)

# At Yarışı (Horse Race)

A horse racing game made with C# Windows Forms for a Visual Programming course assignment. All graphics are drawn in code with GDI+.

## How to play
- Click one of the cards at the bottom to pick the horse you think will win: Kızıl Yıldırım, Beyaz Rüzgar or Kara Şimşek.
- Press **BAŞLAT** (Start): after a 3, 2, 1 countdown the race begins.
- Each horse's speed changes randomly during the race; the finishing order and race time are shown at the end.
- Press **TEKRAR** (Again) for a new race.

## Running it
- **Windows:** open `gorsel programlama 2 - ders1 - at yarışı.slnx` in Visual Studio and press F5 (.NET Framework 4.7.2).
- **Quick look without Windows:** open `tarayici-onizleme.html` in a browser.

## Files
- `Form1.cs` — game logic, timer and all drawing (track, fences, horses and jockeys)
- `Form1.Designer.cs` — form and button layout
- `Program.cs` — entry point
