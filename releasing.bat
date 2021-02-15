:: PosLib
:: 
:: A library for storing and loading application's window positioning.
:: Copyright (C) 2017 VPKSoft, Petteri Kautonen
:: 
:: Contact: vpksoft@vpksoft.net
:: 
:: This file is part of PosLib.
:: 
:: PosLib is free software: you can redistribute it and/or modify
:: it under the terms of the GNU Lesser General Public License as published by
:: the Free Software Foundation, either version 3 of the License, or
:: (at your option) any later version.

:: PosLib is distributed in the hope that it will be useful,
:: but WITHOUT ANY WARRANTY; without even the implied warranty of
:: MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
:: GNU Lesser General Public License for more details.
::
:: You should have received a copy of the GNU Lesser General Public License
:: along with PosLib.  If not, see <http://www.gnu.org/licenses/>.

copy .\PosLib\bin\Release\VPKSoft.PosLib.dll .\PosLib\bin\poslib_release\VPKSoft.PosLib.dll
copy .\PosLib\bin\Release\VPKSoft.PosLib.xml .\PosLib\bin\poslib_release\VPKSoft.PosLib.xml
copy .\PosLib\bin\Release\VPKSoft.PosLib.vnml .\PosLib\bin\poslib_release\VPKSoft.PosLib.vnml
copy .\PosTest\bin\Release\PosTest.exe .\PosLib\bin\poslib_release\PosTest.exe
copy .\PosTestWPF\bin\Release\PosTestWPF.exe .\PosLib\bin\poslib_release\PosTestWPF.exe

copy .\VPKSoft.Utils\VPKSoft.Utils.dll .\PosLib\bin\poslib_release\VPKSoft.Utils.dll
copy .\VPKSoft.Utils\VPKSoft.Utils.xml .\PosLib\bin\poslib_release\VPKSoft.Utils.xml

copy .\COPYING .\PosLib\bin\poslib_release\COPYING
copy .\COPYING.LESSER .\PosLib\bin\poslib_release\COPYING.LESSER

pause