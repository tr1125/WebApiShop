const message = document.querySelector(".msg")
let oldUser = JSON.parse(sessionStorage.getItem("User"))

message.textContent = `hello ${oldUser.firstName}, would you like to update your details?`

const getDetails = () => {
    const updatedUser = {
        userName: document.querySelector(".userName").value || oldUser.userName,
        firstName: document.querySelector(".userfName").value || oldUser.firstName,
        lastName: document.querySelector(".userlName").value || oldUser.lastName,
        password: document.querySelector(".password").value || oldUser.password,
        address: document.querySelector(".userAddress").value || oldUser.address,
        phone: document.querySelector(".userPhone").value || oldUser.phone,
        userId: oldUser.userId
    }
    return updatedUser
}

const updateDetails = async () => {
    const updatedUser = getDetails();
    try {
        const postResponse = await fetch(`/api/users/${oldUser.userId}`, {
            method: 'PUT',
            headers: {
                'Content-type': 'application/json'
            },
            body: JSON.stringify(updatedUser)
        });
         if (!postResponse.ok) {
            alert("something went wrong")
            throw new Error(`error😢! status of error: ${postResponse.status}`);
        }
        sessionStorage.setItem("User", JSON.stringify(updatedUser))
        alert("details updated succesfully")
    }
    catch (e) {
        console.log(e)
    }
}