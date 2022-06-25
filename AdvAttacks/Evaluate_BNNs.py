'''
Версия python 3.8.10
используемые версии модулей python:
pandas==1.3.4
numpy==1.20.2
matplotlib==3.4.1
tensorflow==2.8.0
keras==2.8.0
scikit-learn==0.22.2
torch==1.11.0
torchbnn==1.2
scipy==1.4.1
'''

#импорт необходимых модулей
import torch
import torch.nn as nn
import torch.optim as optim
import torchbnn as bnn
import matplotlib.pyplot as plt
import array
import tensorflow as tf
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import os
from sklearn.model_selection import train_test_split
from tensorflow.keras.utils import to_categorical
from scipy.special import softmax as sci_softmax

#объявление служебных переменных и констант
df_name = '../sign_dump_BNN.csv'
number_of_features_1 = 144
max_type = 22
number_of_neurans_1 = 30
model_name = '../BNN_models/BNN_model_'
id_user='id_user'
users = [1,8,9,10,11,14,15,17,18,19,20,22]

#загрузка датасета
df = pd.read_csv(df_name)
df = df.drop(df.columns[0], axis=1) #удаление служебного безымянного столбца

#получение результата предсказания в softmax encoded виде
def get_predict(test_df):
    #перечень распознаваемых пользователей
    softmax = nn.Softmax(dim=1)

    result = np.zeros((test_df.shape[0], 23))
    
    #для исключения влияния случайных факторов распознавание повторяется 50 раз
    for repeat in range(50):
        #проверка данных на каждом из пользователе
        for i in users: 
            data = test_df
            data_tensor=torch.from_numpy(data).float()
            model = torch.load(model_name+str(i)+'.h5')
            outputs = model(data_tensor)
            prob_array = softmax(outputs).detach().numpy()

            #запись результата в итоговый массив
            for itr in range(test_df.shape[0]):
                result[itr][i] += prob_array[itr][1]

    #приведение результата в softmax encoded вид
    result = sci_softmax(result, axis=1)
    return result

#вычисление значения потерь и точности предсказания относительно верных ответов
def evaluate(prediction, answers):   
    cross_entropy_loss = nn.CrossEntropyLoss()
    
    correct = (np.argmax(prediction, axis=1) == np.argmax(answers, axis=1)).sum()
    accuracy = correct / prediction.shape[0]
    
    pred_tensor = torch.from_numpy(prediction).float()
    ans_tensor = torch.from_numpy(answers).float()
    loss = cross_entropy_loss(pred_tensor, ans_tensor)
    loss = loss.item()

    return loss, accuracy

#определение модели нейронной сети
model = nn.Sequential(
            bnn.BayesLinear(prior_mu=0, prior_sigma=0.01, in_features=144, out_features=300), 
            nn.ReLU(), 
            bnn.BayesLinear(prior_mu=0, prior_sigma=0.01, in_features=300, out_features=2))

#подготовка данных для тестирования
test_df = df[df['id_user'].isin(users)]
answ = test_df['id_user'].to_numpy()
answ = to_categorical(answ, 23)
test_df = test_df.drop(columns = 'id_user')
test_df = test_df.to_numpy()

#тестирование обученных моделей
for i in range (0,31):
    model_name = '../BNN_models/BNN_model_'+str(i)+'/'
    pred = get_predict(test_df)
    print(evaluate(pred, answ))