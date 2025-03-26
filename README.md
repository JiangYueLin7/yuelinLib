# 原则声明

## 类

**类是面向对象语言一等公民！**

**请多用类思考功能模块开发！**

**类应该尽量小巧单一，复用性高**

**不要盲目修改类,主要是不要盲目添加属性**

**超过 15 个属性请考虑效用性,可读性,简洁性**

## 函数

函数参数控制在四个参数以内，[ATPCS 原则](https://baike.baidu.com/item/ATPCS/10695243)

函数体代码量限制在 100 行以内,增强可读性

如果你真的需要在一个函数体内编写超过 100 行,请考虑编写多个函数体,进行调用

---

函数内部职责单一，越单一越清晰 😎

---

考虑复用性，将参数作为更宽泛的类型，以供协变操作

## 异常

多用 throw 关键字
用单个类不断更新属性值，最后在 finally 里写进 log

**例子 loadingRecord 类**

```charp
    var jobName = configInfo.JobName;
    logger.LogInformation("start executing LoadToStaging jobName:{jobName}");
    DateTime now = DateTime.Now;

    var inputFile = configInfo.Input!.FirstOrDefault();
    var tableName = configInfo.ImportDatabaseInfo!.TableName;
    var fileName = inputFile.FileName;
    var filePath = inputFile.FilePath;
    var delimiter = inputFile.Delimiter;
    var fileEncoding = inputFile.FileEncoding;

    LoadingRecord loadingRecord = new(){
        workDate = DateOnly.FromDateTime(Convert.ToDateTime(processingDateInfo.AsofDate)),
        jobName = jobName,
        loadTimeStamp = now,
        feedFileName = fileName,
    };
    try{
        string deleteSql = $"delete from {tableName}";
        csvToStagingRepository.Delete(deleteSql);

        var dataTable = fileHandler.ReadDataFromCsv(filePath + fileName,Encoding.GetEncoding(fileEncoding), delimiter);

        logger.LogInformation($"totalLineNumber:{dataTable.Rows.Count}");

        txnContext.Database.SqlBulkCopy(dataTable, tableName);

        loadingRecord.loadStatus = JobStates.Succeed.ToString();
        loadingRecord.loadRowCount = dataTable.Rows.Count;
    }
    catch(Exception e){
        logger.LogError(e, "loadToStaging error");
        loadingRecord.loadStatus = JobStates.Failed.ToString();
        loadingRecord.Message = e.Message;
        throw;
    }
    finally{
       loadingRecordRepository.Insert(loadingRecord);
       logger.LogInformation("end executing LoadToStaging jobName:{jobName}");
    }

```

## hardcode

**禁止硬编码**
多使用枚举值

```charp
    public enum JobStates{
        Succeed,
        Failed,
        Processing,
        Error,
        Warning
    }
```

## 命名

### 变量名

1.属性以大驼峰命名法
例子：BasicBootInfo 2.变量以小驼峰命名法
例子：basicBootInfo

### 函数名

1.以动词开始 2.限制在 25 个字符或者 3 个单词以内 3.省略无意义名词

| ✅ Do This        | ❎ Don't do this             |
| ----------------- | ---------------------------- |
| `GetUserInfo`     | `GetProjectNameUserInfo`     |
| `GetUserInfoList` | `ProjectNameUserInfoForList` |

## 代码块设计

😁😁

```charp

    /// 1. 所有变量在最顶部
    /// 2. 非关联变量块要用空行分离

    var configs = GetConfig(args);
    var jobName = configs["job];
    var jobStage = configs["step"];

    var rootFolderPath = ProgramBasicInfo.RunningFolder;

    var provider = BuildServiceProbider(configs, rootFolderPath);

    var logger = provider.GetRequiredService<ILogger<Program>>();
    var jobScheduler = provider.GetRequiredService<IJobScheduler>();
    var jobExecutor = provider.GetRequiredService<IJobExecutor>();
    var jobConfigRepository = provider.GetRequiredService<IJobConfigRepository>();

    logger.LogInformation("start executing jobName:{jobName}");

    var bootInfo = new JobBootInfo{JobName = jobName, JobStage = jobStage};

    var configInfo = jobConfigRepository.GetJobConfig(bootInfo);
```

## 注释

除非你的代码本身非常高易读，
否则请在你的类、属性、函数前使用 **summary** 注释，
另外请使用 /// 唤起 **summary** 注释

啰嗦和含糊不清同样可怕

注释的可读性由非作者评定，如果可读性差，请协助一起提高可读性，别让沟通成我们的障碍 🙃
换言之，每个上线的**文档性注释**，都应该由至少两人编写/负责

## 重构代码

在功能实现后，请重构你的代码，
使用**ctrl+r+g** 删除并排序 using 引用

使用**shift+alt+f** 格式化你的代码以保证代码样式

删除多余的空行(不是作为分隔代码块的)

每一个结果都高度可视化

使用!消除编辑器可空警告 (你都不能保证这里是否有空值你的代码是否就是有问题的？)

| ✅ Do This

```charp
    var emailScript = configInfo.Email!.EmailScript!;
    var sqlPhare = GetSqlsFromFile(emailScript);
    var dt = sendEmailRepository.GetSendEmailData(sqlPhare);
```

|❎ Don't do this |

```charp
    var dt = sendEmailRepository.GetSendEmailData(GetSqlsFromFile(configInfo.Email!.EmailScript!));
```

参数清晰，如果你一行非常长，以至于**出现滚动条**，请换行
| ✅ Do This

```charp
    emailHandler.SendEmail(bootInfo,
                            configInfo,
                            emailInfo,
                            sqlPhare,
                            dt,
                            logger,
                            jobScheduler,
                            jobExecutor);

```

|❎ Don't do this |

```charp
    emailHandler.SendEmail(bootInfo, configInfo, emailInfo, GetSqlsFromFile(configInfo.Email!.EmailScript!), sendEmailRepository.GetSendEmailData(GetSqlsFromFile(configInfo.Email!.EmailScript!)), logger, jobScheduler, jobExecutor);
```

参数排序：引用类型在值类型的前面，除非参数排序由特殊含义，否则参数能代表的含义越多就越应该排在前面

| ✅ Do This

```charp
    foo(class A,List<string> b, string c,int b, bool f)
```

|❎ Don't do this |

```charp
    foo(int b, bool f, List<string> b, string c,class A)
```
