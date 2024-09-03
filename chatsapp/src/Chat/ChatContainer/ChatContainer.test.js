import React from 'react';
import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom/extend-expect';
import ChatContainer from './index.js';

describe('ChatContainer', () => {
    const mockCurrentContact = 'John Doe' ;
    const mockMessages = [{ id: "1", sender: 'user', message: 'Hello' }];
    const mockHandleFileDownload = jest.fn();
    const mockHandleMessageSend = jest.fn();
    const mockHandleFileUpload = jest.fn();
  
    test('renders ChatHeader with correct props', () => {
      render(
        <ChatContainer
          currentContact={mockCurrentContact}
          messages={mockMessages}
          handleFileDownload={mockHandleFileDownload}
          currentRecipentID={"1"}
          inputMessage=""
          onInputMessageChange={() => {}}
          onKeyPress={() => {}}
          handleMessageSend={mockHandleMessageSend}
          handleFileUpload={mockHandleFileUpload}
        />
      );
  
      const chatHeader = screen.getByRole('heading', { name: /john doe/i });
      expect(chatHeader).toBeInTheDocument();
    });
  
    test('renders ChatMessages with correct props', () => {
      render(
        <ChatContainer
          currentContact={mockCurrentContact}
          messages={mockMessages}
          handleFileDownload={mockHandleFileDownload}
          currentRecipentID={"1"}
          inputMessage=""
          onInputMessageChange={() => {}}
          onKeyPress={() => {}}
          handleMessageSend={mockHandleMessageSend}
          handleFileUpload={mockHandleFileUpload}
        />
      );
  
      const chatMessages = screen.getByTestId('chat-messages');
      expect(chatMessages).toBeInTheDocument();
    });
  
    test('renders ChatInput with correct props', () => {
      render(
        <ChatContainer
          currentContact={mockCurrentContact}
          messages={mockMessages}
          handleFileDownload={mockHandleFileDownload}
          currentRecipentID={"1"}
          inputMessage=""
          onInputMessageChange={() => {}}
          onKeyPress={() => {}}
          handleMessageSend={mockHandleMessageSend}
          handleFileUpload={mockHandleFileUpload}
        />
      );
  
      const chatInput = screen.getByTestId('chat-input');
      expect(chatInput).toBeInTheDocument();
    });
  });