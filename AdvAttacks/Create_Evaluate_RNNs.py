'''
Версия python 3.8.10
используемые версии модулей python:
pandas==1.3.4
numpy==1.20.2
matplotlib==3.4.1
tensorflow==2.8.0
keras==2.8.0
scikit-learn==0.22.2
'''

#импорт необходимых модулей
import pandas as pd
import numpy as np
import collections
import matplotlib.pyplot as plt
import pylab
import os
import tensorflow as tf
from tensorflow import keras
from tensorflow.keras.models import Sequential, model_from_json
from tensorflow.keras import layers
from tensorflow.keras.layers import Dense, Dropout, GRU, Flatten, SimpleRNN, LSTM, Input, Reshape, Embedding, IntegerLookup
from tensorflow.keras.callbacks import ModelCheckpoint
from tensorflow.keras import backend as K
from tensorflow.keras.utils import to_categorical
from sklearn.model_selection import train_test_split

#объявление служебных переменных и констант
df_name = '../sign_dump_RNN.csv'
number_of_features_1 = 144
max_type = 22
number_of_neurans_1 = 30
model_name = '../RNN_models/RNN_model_'
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

#разбиение на тренировочную и тестовую выборки
X_train, X_test = train_test_split(feed_df, test_size=0.3)
Y_train = X_train[id_user]
Y_train = to_categorical(Y_train)
X_train = X_train.drop(columns=id_user)
Y_test = X_test[id_user]
Y_test = to_categorical(Y_test)
X_test = X_test.drop(columns=id_user)

#определение модели нейронной сети
def Sign_model():
    model = Sequential()
    model.add(Input((144,), name='FeatureInput'))
    model.add(Embedding(input_dim=101, 
                        output_dim=30, 
                        input_length=144,
                        embeddings_initializer=keras.initializers.RandomNormal(),
                        embeddings_regularizer=keras.regularizers.L2(),
                        embeddings_constraint=keras.constraints.NonNeg(),
                        name="Embedding_layer"))
    model.add(GRU(30,
                  return_sequences=True, 
                  reset_after=True,
                  recurrent_dropout = 0.1,
                  dropout = 0.1,
                  name="GRU_layer"))
    model.add(SimpleRNN(30,
                        activation = 'sigmoid',
                        recurrent_dropout = 0.1,
                        dropout = 0.1,
                        name='RNN_layer'))
    model.add(Dense(max_type+1,activation='softmax',name="output_layer"))
    return model

#обучение и сохранение тридцати моделей
for i in range (30):
    model = Sign_model()
    model.compile(loss='categorical_crossentropy',
                  optimizer='adam',
                  metrics=['categorical_accuracy'])
    model.summary()

    results = model.fit(X_train,Y_train,
                        epochs=30,
                        verbose=1,
                        validation_data = (X_test,Y_test))
    model.save_weights(model_name+str(i)+'.h5')
    
    #отрисовка результатов обучения
    plt.plot(results.history['categorical_accuracy'])
    plt.plot(results.history['val_categorical_accuracy'])
    plt.title('Model accuracy')
    plt.ylabel('Accuracy')
    plt.xlabel('Epoch')
    plt.legend(['Train', 'Test'], loc='upper left')
    plt.show()
    
    print(model.evaluate(X_test,Y_test))