   M S B A M L   `   `   `  ���� B  >ChessUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null!clr-namespace:ChessUIChessUI      ChessUI.MainWindow   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     'localclr-namespace:ChessUI       � Title$  Chess��$
��1100��$	��700��$	��950��$	��650�� � Icon$ Assets/icon.ico�� � WindowStartupLocation$ CenterScreen=����#2F2F2F.��� .+���  �� Orientation$ 
Horizontal=�$��Center=�$��Center=�$��20q�.+��� $��0,0,20,0q�$��Center=�$��Center=�.+��� $��BLACK��$��36(�$��Merida)�$
��Bold"�
���   �$��Center=�$��	-60,0,0,5q��� -    ��Name$ BlackClockText��$��10:00��$��44(�$��SemiBold"����#C5B358$��Center=�$��
-60,0,0,30q��� $��WHITE��$��32(�$
��Bold"�$��Merida)�
�������$��Center=�$��	-60,0,0,5q��� -   $ WhiteClockText��$��10:00��$��44(�$��SemiBold"����#C5B358$��Center=�$��	-60,0,0,5q�(� $	��600��$	��600��.��� -    �Name$ MainContainer��.+�� -   $ 	BoardGrid��$	��600��$	��600������  �� ImageSource$ Assets/Board.png��.+�A� -    A�Name$ HighlightGrid�� A� Rows$ 8��	 A� Columns$	 8��A� -   $ 	PieceGrid��$ 8��$	 8��
 �� BitmapScalingMode$
 HighQuality=��� -    ��Name$ MenuContainer��$	��400��$	��200�� K� ZIndex$	 100��$��Center=�$��Center=�$y�	Collapsed=� J� Effect 3  -System.Windows.Media.Effects.DropShadowEffect     
BlurRadius$ 20Q�   ShadowDepth$ 5Q�   Color$ 	#80000000���� $��20,0,0,0q����#FFD700$��4q� �� CornerRadius$ 9�����#202020 �� Padding$ 10q�$	��Top=�   $ 8Q�$ 3Q�$ 	#80000000��._��� $��Center=�.+��� $��SkyeKnet�����#C5B358$��Segoe UI)�$��18(�$��DemiBold"�$��Center=�$��0,0,0,8q� K� Capitals$ 	SmallCaps=� K� NumeralStyle$ OldStyle=�&   System.Windows.Media.TextOptions   TextFormattingMode$ Display=�   $ 6Q�$ 3Q�$ 	#40000000��   	Direction$	 270Q��� $��Assets/Chess_bot_dp.png��$	��170��$	��225��$
 HighQuality=�$��Center=� J� Clip ��  �� Rect$ 0,25,150,125��� ���#303030$ 5��$ 8q�$��0,10,0,0q�$��Center=�$	��180��._��� -   $ BotDialogueText��$�� ��$��14(�$��SemiBold"�$��Merida)�
�������$
��Wrap=� �� TextAlignment$ Center=�$��Center=�$y�	Collapsed=��� $ 
Horizontal=�$��Center=�$��0,10,0,0q�.+��� -	   $
��🚫��$��90��$	��6,2q����#BEBEBE
���   ��� -
   $
��Exit��$��90��$	��6,2q����#BEBEBE
���  ��