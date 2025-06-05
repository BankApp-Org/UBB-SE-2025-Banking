// Message Templates JavaScript

class MessageTemplates {
    constructor() {
        this.initializeEventListeners();
    }

    initializeEventListeners() {
        // Handle message menu buttons
        document.addEventListener('click', (e) => {
            if (e.target.closest('.message-menu-btn')) {
                this.toggleMessageMenu(e);
            } else if (!e.target.closest('.message-menu')) {
                this.closeAllMenus();
            }
        });

        // Handle menu items
        document.addEventListener('click', (e) => {
            const menuItem = e.target.closest('.message-menu-item');
            if (menuItem) {
                this.handleMenuItemClick(menuItem);
            }
        });
    }

    toggleMessageMenu(event) {
        const button = event.target.closest('.message-menu-btn');
        const messageContainer = button.closest('.message-container');
        const existingMenu = messageContainer.querySelector('.message-menu');

        // Close all other menus first
        this.closeAllMenus();

        if (!existingMenu) {
            const menu = this.createMessageMenu(button);
            messageContainer.appendChild(menu);
        }
    }

    createMessageMenu(button) {
        const menu = document.createElement('div');
        menu.className = 'message-menu';
        
        // Determine if this is a right-aligned message (sender's own message)
        const isRightMessage = button.closest('.message-container').classList.contains('right');
        
        if (isRightMessage) {
            menu.innerHTML = `
                <div class="message-menu-item" data-action="delete">
                    <i class="fas fa-trash"></i> Delete
                </div>
                <div class="message-menu-item" data-action="report">
                    <i class="fas fa-flag"></i> Report
                </div>
            `;
        } else {
            menu.innerHTML = `
                <div class="message-menu-item" data-action="report">
                    <i class="fas fa-flag"></i> Report
                </div>
            `;
        }

        return menu;
    }

    closeAllMenus() {
        document.querySelectorAll('.message-menu').forEach(menu => menu.remove());
    }

    handleMenuItemClick(menuItem) {
        const action = menuItem.dataset.action;
        const messageContainer = menuItem.closest('.message-container');
        const messageId = messageContainer.dataset.messageId;

        switch (action) {
            case 'delete':
                this.deleteMessage(messageId);
                break;
            case 'report':
                this.reportMessage(messageId);
                break;
        }

        this.closeAllMenus();
    }

    async deleteMessage(messageId) {
        try {
            const response = await fetch(`/api/messages/${messageId}`, {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const messageContainer = document.querySelector(`[data-message-id="${messageId}"]`);
                messageContainer.remove();
            } else {
                console.error('Failed to delete message');
            }
        } catch (error) {
            console.error('Error deleting message:', error);
        }
    }

    async reportMessage(messageId) {
        try {
            const response = await fetch(`/api/messages/${messageId}/report`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                // Show success notification
                alert('Message reported successfully');
            } else {
                console.error('Failed to report message');
            }
        } catch (error) {
            console.error('Error reporting message:', error);
        }
    }

    async acceptRequest(messageId) {
        try {
            const response = await fetch(`/api/messages/${messageId}/accept`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                // Show success notification
                alert('Request accepted successfully');
            } else {
                console.error('Failed to accept request');
            }
        } catch (error) {
            console.error('Error accepting request:', error);
        }
    }
}

// Initialize message templates when the DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.messageTemplates = new MessageTemplates();
}); 