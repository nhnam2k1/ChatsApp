import React from 'react';
import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom/extend-expect';
import ChatMessages from './index.js';

describe('ChatMessages', () => {
  const mockMessages = [
    { id: 1, sender: 'user', message: 'Hello' },
    { id: 2, sender: 'receiver', message: 'Hi' }
  ];
  const mockHandleFileDownload = jest.fn();

  test('renders messages correctly', () => {
    render(
      <ChatMessages messages={mockMessages} handleFileDownload={mockHandleFileDownload} />
    );

    const chatMessages = screen.getByTestId('chat-messages');
    expect(chatMessages).toBeInTheDocument();

    // Ensure all messages are rendered
    expect(screen.getAllByTestId('message')).toHaveLength(mockMessages.length);
  });

  test('renders messages with correct content', () => {
    render(
      <ChatMessages messages={mockMessages} handleFileDownload={mockHandleFileDownload} />
    );

    const messageElements = screen.getAllByTestId('message');
    messageElements.forEach((messageElement, index) => {
      expect(messageElement).toHaveTextContent(mockMessages[index].message);
    });
  });
});