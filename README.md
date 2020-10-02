# AboveAverageInspector
A custom inspector drawer, because I wanted something I could organize into groups

### NOTE : Above-Average Inspector is a work in progress! 

While the majority of the functionality is present, there more assuredly are still bugs.  
(Known-Issue: Currently, the List items [ + ] Add and [ - ] Remove buttons are not working properly)

| Original Inspector                   | With Above-Average Inspector         |
| ------------------------------------ | ------------------------------------ |
| ![](https://i.imgur.com/aQz7LIb.png) | ![](https://i.imgur.com/8U6SGr9.png) |

### Categorize your fields
Use the ```[UICategory(name:"", order:0, expand:true)]``` attribute to draw your field in a category, order the categories, and decide if you want the category expanded by default. 
![](https://i.imgur.com/x3DCj9e.png)

Uncategorized fields are automatically placed in a "Default" category.

![](https://i.imgur.com/E0amcGN.png)

---
![alt text](https://i.imgur.com/cg5ow2M.png "instance.id")
