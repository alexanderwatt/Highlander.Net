/**
 * Add two numbers
 * @customfunction 
 * @param {number} first First number
 * @param {number} second Second number
 * @returns {number} The sum of the two numbers.
 */
function add(first, second) {
  return first + second;
}

/**
 * Displays the current time once a second
 * @customfunction 
 * @param {CustomFunctions.StreamingInvocation<string>} invocation Custom function invocation
 */
function clock(invocation) {
  const timer = setInterval(() => {
    const time = currentTime();
    invocation.setResult(time);
  }, 1000);

  invocation.onCanceled = () => {
    clearInterval(timer);
  };
}

/**
 * Returns the current time
 * @returns {string} String with the current time formatted for the current locale.
 */
function currentTime() {
  return new Date().toLocaleTimeString();
}

/**
 * Simulates rolling a 6-sided dice.
 * @customfunction
 * @volatile
 */
function roll6sided() {
    return Math.floor(Math.random() * 6) + 1;
}

/**
 * Increments a value once a second.
 * @customfunction 
 * @param {number} incrementBy Amount to increment
 * @param {CustomFunctions.StreamingInvocation<number>} invocation
 */
function increment(incrementBy, invocation) {
  let result = 0;
  const timer = setInterval(() => {
    result += incrementBy;
    invocation.setResult(result);
  }, 1000);

  invocation.onCanceled = () => {
    clearInterval(timer);
  };
}

/**
 * Requests the names of the people currently on the International Space Station from a hypothetical API.
 * @customfunction
 */
function webRequest() {
    let url = "https://www.contoso.com/NumberOfPeopleInSpace";
    return new Promise(function (resolve, reject) {
        fetch(url)
            .then(function (response) {
                    return response.json();
                }
            )
            .then(function (json) {
                resolve(JSON.stringify(json.names));
            })
    })
}

/**
  * Gets the star count for a given Github repository.
  * @customfunction 
  * @param {string} userName string name of Github user or organization.
  * @param {string} repoName string name of the Github repository.
  * @return {number} number of stars given to a Github repository.
  */
async function getStarCount(userName, repoName) {
    try {
        //You can change this URL to any web request you want to work with.
        const url = "https://api.github.com/repos/" + userName + "/" + repoName;
        const response = await fetch(url);
        //Expect that status code is in 200-299 range
        if (!response.ok) {
            throw new Error(response.statusText);
        }
        const jsonResponse = await response.json();
        return jsonResponse.watchers_count;
    }
    catch (error) {
        return error;
    }
}

/**
 * @customfunction
 * @param address range's address
 **/
async function getRangeValue(address) {
    var context = new window.Excel.RequestContext();
    var range = context.workbook.worksheets.getActiveWorksheet().getRange(address);
    range.load();
    await context.sync();
    return range.values[0][0];
}

/**
 * Get text values that spill down.
 * @customfunction
 * @returns {string[][]} A dynamic array with multiple results.
 */
function spillDown() {
    return [['first'], ['second'], ['third']];
}

/**
 * Get text values that spill to the right.
 * @customfunction
 * @returns {string[][]} A dynamic array with multiple results.
 */
function spillRight() {
    return [['first', 'second', 'third']];
}

/**
 * Get text values that spill both right and down.
 * @customfunction
 * @returns {string[][]} A dynamic array with multiple results.
 */
function spillRectangle() {
    return [
        ['apples', 1, 'pounds'],
        ['oranges', 3, 'pounds'],
        ['pears', 5, 'crates']
    ];
}

///**
// * Stores a key-value pair into OfficeRuntime.storage.
// * @customfunction
// * @param {string} key Key of item to put into storage.
// * @param {*} value Value of item to put into storage.
// */
//function storeValue(key, value) {
//    return window.OfficeRuntime.storage.setItem(key, value).then(function (result) {
//        return "Success: Item with key '" + key + "' saved to storage.";
//    }, function (error) {
//        return "Error: Unable to save item with key '" + key + "' to storage. " + error;
//    });
//}

///**
// * Read a token from storage.
// * @customfunction GETTOKEN
// */
//function receiveTokenFromCustomFunction() {
//    var key = "token";
//    var tokenSendStatus = document.getElementById('tokenSendStatus');
//    window.OfficeRuntime.storage.getItem(key).then(function (result) {
//        tokenSendStatus.value = "Success: Item with key '" + key + "' read from storage.";
//        document.getElementById('tokenTextBox2').value = result;
//    }, function (error) {
//        tokenSendStatus.value = "Error: Unable to read item with key '" + key + "' from storage. " + error;
//    });
//}

/**
 * Function retrieves a cached token or opens a dialog box if there is no saved token. Note that this is not a sufficient example of authentication but is intended to show the capabilities of the Dialog object.
 * @param {string} url URL for a stored token.
 */
function getTokenViaDialog(url) {
    return new Promise(function (resolve, reject) {
        if (_dialogOpen) {
            // Can only have one dialog box open at once. Wait for previous dialog box's token.
            let timeout = 5;
            let count = 0;
            var intervalId = setInterval(function () {
                count++;
                if (_cachedToken) {
                    resolve(_cachedToken);
                    clearInterval(intervalId);
                }
                if (count >= timeout) {
                    reject("Timeout while waiting for token");
                    clearInterval(intervalId);
                }
            }, 1000);
        } else {
            _dialogOpen = true;
            OfficeRuntime.displayWebDialog(url, {
                height: '50%',
                width: '50%',
                onMessage: function (message, dialog) {
                    _cachedToken = message;
                    resolve(message);
                    dialog.close();
                    return;
                },
                onRuntimeError: function (error, dialog) {
                    reject(error);
                },
            }).catch(function (e) {
                reject(e);
            });
        }
    });
}

/**
 * Returns the volume of a sphere.
 * @customfunction
 * @param {number} radius
 */
function sphereVolume(radius) {
    return Math.pow(radius, 3) * 4 * Math.PI / 3;
}

/**
 * Gets a comment from the hypothetical contoso.com/comments API.
 * @customfunction
 * @param {number} commentId ID of a comment.
 */
function getComment(commentId) {
    let url = "https://www.contoso.com/comments/" + commentId;
    return fetch(url)
        .then(function (data) {
            return data.json();
        })
        .then(function (json) {
            return json.body;
        })
        .catch(function (error) {
            throw new window.CustomFunctions.Error(window.CustomFunctions.ErrorCode.notAvailable);
        });
}

/**
 * Writes a message to console.log().
 * @customfunction LOG
 * @param {string} message String to write.
 * @returns String to write.
 */
function logMessage(message) {
  console.log(message);
  return message;
}
