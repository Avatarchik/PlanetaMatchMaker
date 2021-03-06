#pragma once

#include <memory>

#include <boost/asio.hpp>
#include <boost/noncopyable.hpp>

#include "./server_setting.hpp"
#include "./server_data.hpp"

namespace pgl {
	class server final : boost::noncopyable {
	public:
		explicit server(std::unique_ptr<server_setting>&& setting);
		void run();
	private:
		boost::asio::io_service io_service_;
		boost::asio::ip::tcp::acceptor acceptor_;
		std::unique_ptr<server_data> server_data_;
		std::unique_ptr<server_setting> server_setting_;
	};
}
