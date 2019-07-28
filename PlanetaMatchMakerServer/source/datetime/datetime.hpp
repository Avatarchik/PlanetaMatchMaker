#pragma once

#include <cstdint>
#include <string>

#include <boost/functional/hash.hpp>
#include <boost/operators.hpp>

#include "serialize/serializer.hpp"

namespace pgl {
	// 8 bytes
	struct datetime final : private boost::less_than_comparable<datetime>, boost::equality_comparable<datetime> {
		datetime() = default;
		datetime(int year, int month, int day);
		datetime(int year, int month, int day, int hour, int minute, int second);

		[[nodiscard]] int year() const;

		[[nodiscard]] int month() const;

		[[nodiscard]] int day() const;

		[[nodiscard]] int hour() const;

		[[nodiscard]] int minute() const;

		[[nodiscard]] int second() const;

		[[nodiscard]] size_t get_hash() const;

		bool operator<(const datetime& other) const;

		bool operator==(const datetime& other) const;

		static datetime now();

		void on_serialize(serializer& serializer) {
			serializer += unix_time_;
		}

	private:
		int64_t unix_time_;
	};

	std::string get_now_datetime_string();
}

namespace boost {
	inline size_t hash_value(const pgl::datetime& datetime) {
		return datetime.get_hash();
	}
}

namespace std {
	template <>
	struct hash<pgl::datetime> {
		size_t operator()(const pgl::datetime& datetime) const noexcept {
			return boost::hash_value(datetime);
		}
	};
}
