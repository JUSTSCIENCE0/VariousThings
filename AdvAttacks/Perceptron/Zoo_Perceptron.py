'''
Версия python 3.8.10
используемые версии модулей python:
pandas==1.3.4
numpy==1.20.2
matplotlib==3.4.1
tensorflow==2.8.0
keras==2.8.0
scikit-learn==0.22.2
adversarial-robustness-toolbox==1.10.0
'''

#импорт необходимых модулей
import pandas as pd
import numpy as np
import collections
import matplotlib.pyplot as plt
import pylab
import random
import os
import tensorflow as tf
from tensorflow import keras
from tensorflow.keras.models import Sequential, model_from_json
from tensorflow.keras import layers
from tensorflow.keras.layers import Dense, Dropout, GaussianNoise, Lambda, Flatten, Conv2D, MaxPooling2D, Input, Reshape
from tensorflow.keras.callbacks import ModelCheckpoint
from tensorflow.keras import backend as K
from tensorflow.keras.utils import to_categorical
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import LabelBinarizer
from art.attacks.evasion import ZooAttack
from art.estimators.classification import BlackBoxClassifierNeuralNetwork

#объявление служебных переменных и констант
df_name = '../sign_dump_Perc.csv'
model_name = '../Perc_models/Perc_model_'
id_user='id_user'

#загрузка датасета
df = pd.read_csv(df_name)
df = df.drop(df.columns[0], axis=1) #удаление служебного безымянного столбца

#выделение набора легальных и нелегальных пользователей
friend_users = [1,8,9,10,11,14,15,17,18,19,20,22]
alien_users = [2,3,4,5,6,7,12,16,21]
friend_df = df[df[id_user].isin(friend_users)]
alien_df  = df[df[id_user].isin(alien_users)]
alien_df[id_user]=0
feed_df=friend_df.merge(alien_df,how='outer')

#подготовка данных для атаки
feed_df = friend_df.merge(alien_df,how='outer')
gradient_df = feed_df.copy()
X_for_attack = gradient_df.drop(columns='id_user')
X_for_attack = X_for_attack.to_numpy()
Y_for_attack = gradient_df['id_user']
Y_for_attack = to_categorical(Y_for_attack, num_classes = 23)

#определение модели нейронной сети
def Sign_model():
    model = Sequential()
    model.add(Input((144), name="input_layer"))                   # 144 - количество входных параметров
    model.add(Dense(30,activation='relu', name="hidden_layer"))   # 30  - количество нейронов в промежуточном слое
    model.add(Dense(23,activation='softmax',name="output_layer")) # 23  - количество всех юзеров
    return model

#функция получения предсказаний
def get_prediction(x):
    result = model.predict(x)
    return result

#проведение атаки для обученных датасетов
for iteration in range(30):
    print('iteration №', iteration + 1)
    model = Sign_model()
    model.compile(loss='categorical_crossentropy',
        optimizer='adam',
        metrics=['categorical_accuracy'])
    name = model_name+str(i)+'.h5'
    model.load_weights(name)
    
    classifier = BlackBoxClassifier(predict = get_prediction, 
                                    nb_classes = 23, 
                                    input_shape=(144,))
    
    attack =  ZooAttack(classifier, max_iter = 1)
    x_test_adv = attack.generate(x=X_for_attack, y=Y_for_attack)
    print(model.evaluate(x_test_adv, Y_for_attack))