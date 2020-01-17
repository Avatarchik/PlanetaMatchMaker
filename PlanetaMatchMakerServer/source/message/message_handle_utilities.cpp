#include "message_handle_utilities.hpp"
#include <utility>

namespace pgl {
	bool does_room_group_exist(const std::shared_ptr<message_handle_parameter> param,
		room_group_index_t room_group_index) {
		// Check if the id is valid
		if (param->server_data.is_valid_room_group_index(room_group_index)) {
			log_with_endpoint(log_level::debug, param->socket.remote_endpoint(), "The room group index \"",
				room_group_index, "\" exists.");
			return true;
		}

		// Send room group index doesn't exist error to the client
		log_with_endpoint(log_level::error, param->socket.remote_endpoint(), "The room group index \"",
			room_group_index, "\" doesn't exist. Range of valid room group index is 0 to ",
			param->server_data.room_group_count(), ".");
		return false;
	}

	void check_room_group_existence(const std::shared_ptr<message_handle_parameter> param,
		const room_group_index_t room_group_index) {
		// Check if the id is valid
		if (does_room_group_exist(param, room_group_index)) { return; }

		const auto error_message = minimal_serializer::generate_string("The room group with index \"", room_group_index,
			"\" does not exist.");
		throw server_session_error(server_session_error_code::continuable_error, error_message);
	}

	bool does_room_exist(const std::shared_ptr<message_handle_parameter> param,
		const room_data_container& room_data_container, room_id_t room_id) {
		// Check room existence
		if (room_data_container.is_data_exist(room_id)) {
			log_with_endpoint(log_level::debug, param->socket.remote_endpoint(), "The room whose id is \"", room_id,
				"\" exists.");
			return true;
		}

		// Send room doesn't exist error to the client
		log_with_endpoint(log_level::error, param->socket.remote_endpoint(), "The room whose id is \"", room_id,
			"\" doesn't exist.");
		return false;
	}

	void check_room_existence(std::shared_ptr<message_handle_parameter> param,
		const room_data_container& room_data_container, const room_id_t room_id) {
		// Check room existence
		if (does_room_exist(std::move(param), room_data_container, room_id)) { return; }

		// Send room doesn't exist error to the client
		const auto error_message = minimal_serializer::generate_string("The room with id \"", room_id,
			"\" does not exist.");
		throw server_session_error(server_session_error_code::continuable_error, error_message);
	}
}
