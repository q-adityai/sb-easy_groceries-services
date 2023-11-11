# Easy Groceries Services



# Basket State Transitions
![image](https://github.com/q-adityai/sb-easy_groceries-services/assets/109814430/bee4c233-11b9-4dd9-b5e4-b58029b36721)



```
---
title: Basket State transitions
---
stateDiagram-v2
    [*] --> Empty : BasketCreated

    Empty --> Active: ProductAddedToBasket    
    Active --> Active: ProductAddedToBasket, ProductRemovedFromBasket(not last)
    Active --> Empty: ProductRemovedFromBasket (last)
    Active --> Deleted: BasketDelete
    Active --> Suspended: UserInactive
    Suspended --> Active: UserActive
    Active --> CheckedOut: BasketCheckout
    CheckedOut --> Active: CheckoutCancel
    CheckedOut --> Locked: InitiateOrderPayment
    Locked --> Locked: PaymentFailure
    Locked --> Closed: PaymentSuccess

    Deleted --> [*]
    Suspended --> [*]
    Closed --> [*]
```

# Order State Transitions
![image](https://github.com/q-adityai/sb-easy_groceries-services/assets/109814430/a41b3a87-f232-4d29-8d83-4c12b9137b6a)


```
---
title: Order State transitions
---
stateDiagram-v2
    [*] --> Created : BasketCheckout

    Created --> Cancelled: OrderCancelled, CheckoutCancel, BasketDelete
    Created --> SentForPayment: InitiateOrderPayment
    SentForPayment --> PaymentSuccessful: PaymentSuccess
    SentForPayment --> PaymentFailed: PaymentFailure
    PaymentSuccessful --> Dispatched: DispatchOrder

    PaymentFailed --> SentForPayment: InitiateOrderPayment

    Cancelled --> [*]
    Dispatched --> [*]
    PaymentFailed --> [*]
    
```
