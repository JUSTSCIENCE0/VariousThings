'''
Версия python 3.8.10
используемые версии модулей python:
pandas==1.3.4
numpy==1.20.2
matplotlib==3.4.1
scikit-learn==0.22.2
scipy==1.4.1
tk==0.1.0
tkintertable==1.3.3
'''

#импорт необходимых модулей
from tkinter import *
import pandas as pd
import math
import cmath
from scipy.fft import *
from scipy.signal import *
import matplotlib.pyplot as plt
import numpy as np

#восстановление последовательности по комплексным значениям и частотам
def restore_sequence(modulus, phase, frequences):
    eps=1e-10
    rev_size = math.ceil(100/frequences[0])
    rev_fft=[0] * rev_size

    for i in range(1,len(modulus)):
        rev_fft[frequences[i]] = cmath.rect(modulus[i], phase[i])

    fft_freq = rfftfreq(rev_size*2-2, sampling_period)

    fig, ax = plt.subplots(figsize=(5, 3))
    ax.vlines(x=fft_freq[:17], ymin=0, ymax=np.abs(rev_fft)[:17], color=(0, 0, 0), linewidth=3)
    ax.set_xlabel('частота, Гц')
    ax.set_ylabel('модуль значения БФП')
    ax.grid()
    plt.show()

    res_fft=irfft(rev_fft, math.ceil(200/frequences[0]))
    res_fft[:] = [x + modulus[0] for x in res_fft]
    return res_fft

#восстановление подписи из датафрейма
def restore_from_df(sign_df, number):
    names_modulus_X = []
    names_phases_X  = []
    names_modulus_Y = []
    names_phases_Y  = []
    names_modulus_Z = []
    names_phases_Z  = []
    
    #распределение фич датасета по категориям
    for i in range(number - 1):
        names_modulus_X.append("feature_"+str(2*i+1))
        names_phases_X.append("feature_"+str(2*i+2))
        names_modulus_Y.append("feature_"+str((number-1)*2+2*i+1))
        names_phases_Y.append("feature_"+str((number-1)*2+2*i+2))
        names_modulus_Z.append("feature_"+str((number-1)*4+2*i+1))
        names_phases_Z.append("feature_"+str((number-1)*4+2*i+2))
    
    modulus_X = [0]
    phases_X = [0]
    frequences_X = [1/number, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]
    modulus_Y = [0]
    phases_Y = [0]
    frequences_Y = [1/number, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]
    modulus_Z = [0]
    phases_Z = [0]
    frequences_Z = [1/number, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]
    
    #подготовка данных к восстановлению последовательностей
    for i in names_modulus_X:
        modulus_X.append(sign_df[i][0])
    for i in names_phases_X:
        phases_X.append(sign_df[i][0])
    for i in names_modulus_Y:
        modulus_Y.append(sign_df[i][0])
    for i in names_phases_Y:
        phases_Y.append(sign_df[i][0])
    for i in names_modulus_Z:
        modulus_Z.append(sign_df[i][0])
    for i in names_phases_Z:
        phases_Z.append(sign_df[i][0])
        
    #восстановление последовательностей
    X = restore_sequence(modulus_X, phases_X, frequences_X)
    Y = restore_sequence(modulus_Y, phases_Y, frequences_Y)
    Z = restore_sequence(modulus_Z, phases_Z, frequences_Z)
    
    #отрисовка последовательностей
    fig, ax = plt.subplots()
    ax.plot(X, color = (0, 0, 0), linewidth = 3)
    ax.set_xlabel('номер отсчета')
    ax.set_ylabel('значение X')
    ax.grid()
    fig.set_figheight(3)
    fig.set_figwidth(5)
    plt.show()
    
    fig, ax = plt.subplots()
    ax.plot(Y, color = (0, 0, 0), linewidth = 3)
    ax.set_xlabel('номер отсчета')
    ax.set_ylabel('значение Y')
    ax.grid()
    fig.set_figheight(3)
    fig.set_figwidth(5)
    plt.show()
    
    fig, ax = plt.subplots()
    ax.plot(Z, color = (0, 0, 0), linewidth = 3)
    ax.set_xlabel('номер отсчета')
    ax.set_ylabel('значение Z')
    ax.grid()
    fig.set_figheight(3)
    fig.set_figwidth(5)
    plt.show()
    
    #отрисовка подписи в целом
    root = Tk()
    canvas = Canvas(root, width=700, height=600, bg='white')
    canvas.pack()
    for i in range(len(X) - 1):
        if (Z[i] < 0):
            line = canvas.create_line((X[i]+150),(Y[i]+200),(X[i+1]+150),(Y[i+1]+200), width=2, fill="black")
    root.mainloop()
    
sign_df = pd.read_csv("attacked_sign.csv")
restore_from_df(sign_df.head(1), 16)