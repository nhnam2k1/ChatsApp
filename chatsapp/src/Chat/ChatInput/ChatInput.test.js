import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom/extend-expect';
import ChatInput from './index.js';

describe('ChatInput', () => {
  const mockCurrentRecipientID = "1";
  const mockInputMessage = '';
  const mockOnInputMessageChange = jest.fn();
  const mockOnKeyPress = jest.fn();
  const mockHandleMessageSend = jest.fn();
  const mockHandleFileUpload = jest.fn();

  test('renders input field correctly', () => {
    render(
      <ChatInput
        currentRecipientID={mockCurrentRecipientID}
        inputMessage={mockInputMessage}
        onInputMessageChange={mockOnInputMessageChange}
        onKeyPress={mockOnKeyPress}
        handleMessageSend={mockHandleMessageSend}
        handleFileUpload={mockHandleFileUpload}
      />
    );

    const inputElement = screen.getByPlaceholderText('Type your message...');
    expect(inputElement).toBeInTheDocument();
    expect(inputElement).toHaveValue(mockInputMessage);
  });

  test('disables input field if no current recipient', () => {
    render(
      <ChatInput
        currentRecipientID={null}
        inputMessage={mockInputMessage}
        onInputMessageChange={mockOnInputMessageChange}
        onKeyPress={mockOnKeyPress}
        handleMessageSend={mockHandleMessageSend}
        handleFileUpload={mockHandleFileUpload}
      />
    );

    const inputElement = screen.getByPlaceholderText('Type your message...');
    expect(inputElement).toBeDisabled();
  });

  test('calls onInputMessageChange when input value changes', () => {
    render(
      <ChatInput
        currentRecipientID={mockCurrentRecipientID}
        inputMessage={mockInputMessage}
        onInputMessageChange={mockOnInputMessageChange}
        onKeyPress={mockOnKeyPress}
        handleMessageSend={mockHandleMessageSend}
        handleFileUpload={mockHandleFileUpload}
      />
    );

    const inputElement = screen.getByPlaceholderText('Type your message...');
    fireEvent.change(inputElement, { target: { value: 'New message' } });
    expect(mockOnInputMessageChange).toHaveBeenCalledTimes(1);
  });
});