   M S B A M L   `   `   `  ���� L  HLinearProgramming, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:LinearProgrammingLinearProgramming  "    LinearProgramming.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     1localclr-namespace:LinearProgramming       � Title$  
MainWindow��$	��800��$
��1400�� � WindowStyle$
 None=� � 
ResizeMode$ NoResize=� � AllowsTransparency . 
������  � WindowStartupLocation$ CenterScreen=�.����  �� CornerRadius$ 5��
���  ��$��1q�
�������._��� .+��� 
���   �$��1q�$��50��$	��250�� �� Left$ 5��
 �� Top$ 5��._��� $��25(�$��Меню��$��Center=�$��Center=��� 
���   �$��1q�$	��733��$	��250��$ 5��$ 60��._��� .+��� $��Метод Гаусса��$��20(�$��50��$	��248��$ 0��
������ $��0,0,0,0q�
���   ��� $!��Симплекс метод��$��20(�$��50��$	��248��$ 50��
������ $��0,1,0,1q�
���   ��� 
���   �$��1q�$	��545��$	��878��$	 260��$ 60��$
��Left=�$��Center=�._��� $��Visible=�$��Visible=�.���� -   $��InputCanvas��$��15��$��15��$	��Top=�$��Center=��� 
���   �$��1q�$	��490��$	��250�� �� Right$ 5��$	 116��$
��Left=�$	��Top=�._��� -   $��	ExtraData��.+��� $	��200��$��50��
���   �$��0,0,1,1q�._��� .+��� $ 0��.+��� $/��)Количество переменных��$��16(�$	��200��$��50��
���   �$��0,0,1,1q�$��Center=��� -   $��xAmountTextBox��$��49��$��5��$��50��$��35(�$	 200��$��0,0,0,1q�
���   �$��Center=�$��Center=�	 �� SelectionOpacity$	 1Q���Z� �� $)��#Количество условий��$��16(�$	��200��$��50��$ 50��
���   �$��0,0,1,1q�$��Center=��� -   $��conditionsAmountTextBox��$��49��$��5��$��50��$��35(�$ 50��$	 200��$��0,0,0,1q�
���   �$��Center=�$��Center=�$	 1Q���Z� �� -   $��addConditionButton��$%��Добавить условие��$��16(�$	��248��$��50��$	 100��
�������
���   �$��0,0,0,1q��� -   $��removeConditionButton��$#��Удалить условия��$��16(�$	��248��$��50��$	 150��
�������
���   �$��0,0,0,1q�$��Center=�$	��Top=��� 
���   �$��1q�$	��183��$
��1133��$	 260��$	 610��$
��Left=�$��Center=��� 
���   �$��1q�$��50��$	��877��$	 260��$ 5��$
��Left=�$��Center=�._��� $��25(�$.��(Входные данные задачи��$��Center=�$��Center=�$��	0,-1,-1,1q��� 
���   �$��1q�$��50��$	��250��$ 5��$ 61��$��Center=�$��Center=�._��� $��20(�$/��)Дополнительные данные��$��Center=�$��Center=��� -   $��
QuitButton��$	��✕��$��30(�$��0q�
���  ��$ 5��$��Center=�$��50��$	��126��$ 5��$
��Left=��� -   $��MinimizeButton��$	��✕��$��30(�$��0q�
��� � �$	 130��$	��Top=�$��50��$	��126��$ 5��