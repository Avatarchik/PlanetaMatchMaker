﻿#pragma once

#include <cstdint>

#include "datetime/datetime.hpp"
#include "client/client_address.hpp"
#include "room/room_constants.hpp"
#include "data/data_constants.hpp"

namespace pgl {
	enum class message_type : uint8_t {
		authentication_request,
		authentication_reply,
		create_room_request,
		create_room_reply,
		list_room_request,
		list_room_reply,
		join_room_request,
		join_room_reply,
		update_room_status_request,
		update_room_status_reply,
		random_match_request
	};

	// 1 bytes
	struct message_header final {
		message_type message_type{};
	};

	// size of message should be less than 256 bytes

	// 2 bytes
	struct authentication_request_message final {
		version_type version{};
	};

	// 3 bytes
	struct authentication_reply_message final {
		enum class error_code : uint8_t {
			ok,
			unknown_error,
			version_mismatch,
			authentication_error,
			denied // when in black list
		};

		error_code error_code{};
		version_type version{};
	};

	// 42 bytes
	struct create_room_request_message final {
		room_name_type name{};
		room_flags_bit_mask::flags_type flags{};
		room_password_type password{};
		uint8_t max_player_count{};
	};

	// 5 bytes
	struct create_room_reply final {
		enum class error_code : uint8_t {
			ok,
			unknown_error,
			room_name_duplicated,
			room_count_reaches_limit
		};

		error_code error_code{};
		room_id_type room_id{};
	};

	// 4 bytes
	struct list_room_request_message final {
		enum class sort_kind : uint8_t {
			name_ascending,
			name_descending,
			create_datetime_ascending,
			create_datetime_descending
		};

		uint8_t start_index{};
		uint8_t end_index{};
		sort_kind sort_kind{};
		uint8_t flags{}; //filter conditions about room
	};

	// 237 bytes
	struct list_room_reply final {
		enum class error_code : uint8_t {
			ok,
			unknown_error,
		};

		//39 bytes
		struct room_info {
			room_id_type room_id{};
			room_name_type name{};
			room_flags_bit_mask::flags_type flags{};
			uint8_t max_player_count{};
			uint8_t current_player_count{};
			datetime create_datetime{};
		};

		error_code error_code{};
		uint8_t total_room_count{};
		uint8_t reply_room_count{};
		room_info room_info_list[6]{};
	};

	// 20 bytes
	struct join_room_request_message final {
		room_id_type room_id{};
		room_password_type password{};
	};

	//19 bytes
	struct join_room_reply_message final {
		enum class error_code : uint8_t {
			ok,
			unknown_error,
			room_not_exist,
			permission_denied,
			join_rejected,
			player_count_reaches_limit
		};

		error_code error_code{};
		client_address host_address{};
	};

	// 5 bytes
	struct update_room_status_request_message final {
		enum class status : uint8_t { open, close, remove };

		room_id_type room_id{};
		status status{};
	};

	// 1 bytes
	struct update_room_status_reply_message final {
		enum class error_code : uint8_t {
			ok,
			unknown_error,
			room_not_exist
		};

		error_code error_code{};
	};

	struct random_match_request_message final {
		enum class error_code : uint8_t {
			ok,
			unknown_error
		};

		error_code error_code{};
	};
}
