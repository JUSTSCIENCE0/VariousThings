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

#обучение и сохранение тридцати моделей
os.mkdir(model_name+str(0)+'/')
for j in range(30):
    #обучение BNN для распознавание каждого отдельного класса
    for i in users: 
        #пометка всех, кроме текущего, пользователей в 0
        friend_users = [int(i)]
        alien_users = users.copy()
        alien_users = np.array(alien_users)
        alien_users = alien_users[alien_users != i]
        alien_users = alien_users.tolist()
        friend_df = df[df['id_user'].isin(friend_users)]
        alien_df  = df[df['id_user'].isin(alien_users)]
        alien_df.loc[:,'id_user']=0
        friend_df.loc[:,'id_user']=1
        feed_alien_df = friend_df.merge(alien_df,how='outer')   
        
        #разбиение на тренировочную и тестовую выборки
        X_train, X_test = train_test_split(feed_alien_df, test_size=0.3)
        Y_train = X_train['id_user']
        X_train = X_train.drop(columns='id_user')
        data = X_train.to_numpy()
        target = Y_train.to_numpy()
        data_tensor=torch.from_numpy(data).float()
        target_tensor=torch.from_numpy(target).long()
        
        #определение модели нейронной сети
        model = nn.Sequential(
            bnn.BayesLinear(prior_mu=0, prior_sigma=0.01, in_features=96, out_features=200), 
            nn.ReLU(),
            bnn.BayesLinear(prior_mu=0, prior_sigma=0.01, in_features=200, out_features=2))
        cross_entropy_loss = nn.CrossEntropyLoss()
        klloss = bnn.BKLLoss(reduction='mean', last_layer_only=False)
        klweight = 0.01
        optimizer = optim.Adam(model.parameters(), lr=0.01)
        
        #обучение 500 эпох
        for step in range(500):
            models = model(data_tensor)
            cross_entropy = cross_entropy_loss(models, target_tensor)
            kl = klloss(model)
            total_cost = cross_entropy + klweight*kl
          
            optimizer.zero_grad()
            total_cost.backward()
            optimizer.step()
            
            ce = cross_entropy.clone()
            kl_val = kl.clone()
            tc = total_cost.clone()
            
            _, predicted = torch.max(models.data, 1)
            final = target_tensor.size(0)
            correct = (predicted == target_tensor).sum()
            accuracy = (100 * float(correct) / final)
            print('Iter #', step+1,
                  ' CE = ', ce.item(),
                  ', KL = ', kl_val.item(),
                  ', TL = ', tc.item(),
                  ', Accuracy: %f %%' % accuracy)
            
            if (accuracy > 0.98 and tc.item()<0.1):
                print('training was completed ahead of schedule by accuracy values')
                break

        torch.save(model, model_name+str(j)+'/'+str(i)+'.h5')
    os.mkdir(model_name+str(j+1)+'/')