rem set path=..\ReplaceAll\bin\Debug
set path=..

%path%\Abstracta.ReplaceAll_NET4.0.exe -i example_00.log -r , -w \t -o "res_00.log"
%path%\Abstracta.ReplaceAll_NET4.0.exe -i example_01.log -r "," -w "\t" -o "res_01.log"
%path%\Abstracta.ReplaceAll_NET4.0.exe -i example_02.log -r " " -w "\t" -o "res_02.log"

rem regex replacement
%path%\Abstracta.ReplaceAll_NET4.0.exe -i example_00.log -x -r ",$" -w "" -o "res_e_00.log"
%path%\Abstracta.ReplaceAll_NET4.0.exe -i example_03.log -x -r "(^\S+) (\[.*\]) (\S+) (\d+) (\d+$)" -w "\t" -o "res_e_01.log"
%path%\Abstracta.ReplaceAll_NET4.0.exe -i example_04.log -x -r "(^\S+ )(\d\d)/(\d\d)/(\d\d\d\d .*$)" -t -w "$1$3/$2/$4" -o "res_e_02.log"

%path%\Abstracta.ReplaceAll_NET4.0.exe -i example_04.log -x -r "(^\S+ )(\d\d)/(\d\d)/(\d\d\d\d .*$)" -t -w "$1$3/$2/$4" -o "res_e_03.log" --fromLine 2 --toLine 3

pause